using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    public float visionRange = 10f;
    public float chaseTime = 5f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public int damage = 1;
    public float turnSpeed = 5f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private float waitTimer = 0f;
    private float lostPlayerTimer = 0f;
    private float attackTimer = 0f;
    private bool chasing = false;
    private bool isAttacking = false;

    private PlayerHealth playerHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentIndex].position);

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        attackTimer += Time.deltaTime;

        if (distanceToPlayer < visionRange)
        {
            // 追击玩家
            chasing = true;
            lostPlayerTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(player.position);

            animator.SetTrigger("walk"); // 播放走路动画

            if (distanceToPlayer <= attackRange && attackTimer >= attackCooldown)
            {
                StartCoroutine(AttackRoutine());
                attackTimer = 0f;
            }
        }
        else if (chasing)
        {
            // 玩家离开视野
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer > chaseTime)
            {
                chasing = false;
                GoToNextWaypoint();
            }
        }
        else
        {
            // 巡逻
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                waitTimer += Time.deltaTime;

                if (waitTimer > waitTime)
                {
                    GoToNextWaypoint();
                    waitTimer = 0f;
                }
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;

        // 面向玩家
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 播放攻击动画
        animator.SetTrigger("attack");

        // 延迟造成伤害（动画节奏配合）
        yield return new WaitForSeconds(0.5f);

        // 攻击判定，支持玩家缩小
        float scaledAttackRange = attackRange * Mathf.Max(player.localScale.x, player.localScale.y, player.localScale.z);
        Collider[] hits = Physics.OverlapSphere(transform.position, scaledAttackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage); // 攻击玩家，血条联动
                }
            }
        }

        // 攻击动画结束后恢复
        yield return new WaitForSeconds(0.7f);

        isAttacking = false;
        if (chasing)
            agent.isStopped = false;
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentIndex = (currentIndex + 1) % waypoints.Length;
        agent.isStopped = false;
        agent.SetDestination(waypoints[currentIndex].position);

        animator.SetTrigger("walk"); // 巡逻走路动画
    }

    // 可视化攻击范围，便于调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float scaledAttackRange = attackRange;
        if (player != null)
            scaledAttackRange *= Mathf.Max(player.localScale.x, player.localScale.y, player.localScale.z);
        Gizmos.DrawWireSphere(transform.position, scaledAttackRange);
    }
}
