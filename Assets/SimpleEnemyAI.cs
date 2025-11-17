using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemyAI : MonoBehaviour
{
    [Header("플레이어 타겟")]
    public Transform player;
    public TargetEntity targetEntity; // 플레이어의 TargetEntity 스크립트

    [Header("설정")]
    public float detectRange = 10f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f; // 공격 시 데미지

    NavMeshAgent agent;
    Animator anim;

    [SerializeField] float attackCooldown = 1.0f; // 연속 공격 최소 간격
    [SerializeField] float turnSpeed = 10f;       // 공격 시 회전 속도

    bool isDead = false;
    bool isAttacking = false;
    float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // targetEntity가 안 넣어져 있으면 자동 탐색
        if (targetEntity == null && player != null)
            targetEntity = player.GetComponent<TargetEntity>();

        // 공격 범위에 안정적으로 진입하도록 정지 거리 보정
        if (agent != null)
            agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, Mathf.Max(0.05f, attackRange * 0.8f));
    }

    void Update()
    {
        if (isDead) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 🔹 플레이어 감지 전 (Idle)
        if (dist > detectRange)
        {
            SetState(idle: true);
            return;
        }

        // 🔹 플레이어 감지 후 (Run)
        if (dist <= detectRange && dist > attackRange)
        {   

            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetState(run: true);
            return;
        }

        // 🔹 공격 범위 진입
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            // 플레이어를 바라보도록 회전
            FaceTarget();

            // 이동/대기 플래그 해제해 공격 전이에 방해 없도록
            anim.SetBool("Run", false);
            anim.SetBool("IdleOne", false);

            TryAttack();
            return;
        }
    }

    void SetState(
        bool idle = false,
        bool run = false,
        bool attack = false,
        bool die = false)
    {
        anim.SetBool("IdleOne", idle);
        anim.SetBool("Run", run);

        // ✅ 공격은 오직 "Hit" 애니메이션만 사용
        anim.SetBool("Hit", attack);

        anim.SetBool("Dies", die);
    }

    void FaceTarget()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }

    void TryAttack()
    {
        if (isDead) return;
        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;
        StartCoroutine(AttackOnce());
    }

    System.Collections.IEnumerator AttackOnce()
    {
        isAttacking = true;
        // Animator의 "Hit" bool을 짧게 펄스해서 전이를 확실히 유도
        anim.SetBool("Hit", true);
        yield return null; // 한 프레임 대기하여 파라미터 반영 보장
        anim.SetBool("Hit", false);
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        agent.isStopped = true;
        SetState(die: true);
    }

    // ✅ 공격 타이밍에 데미지 주는 함수 (애니메이션 이벤트용)
    public void DealDamage()
    {
        if (isDead || targetEntity == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 공격 사정거리 안에 있을 때만 데미지 적용
        if (dist <= attackRange + 0.3f)
        {
            targetEntity.TakeDamage(attackDamage);
        }
    }
}
