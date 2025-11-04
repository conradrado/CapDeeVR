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

    bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // targetEntity가 안 넣어져 있으면 자동 탐색
        if (targetEntity == null && player != null)
            targetEntity = player.GetComponent<TargetEntity>();
    }

    void Update()
    {
        if (isDead) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 🔹 플레이어 감지 전 (Idle)
        if (dist > detectRange)
        {
            Debug.Log("플레이어 감지 해제");
            SetState(idle: true);
            return;
        }

        // 🔹 플레이어 감지 후 (Run)
        if (dist <= detectRange && dist > attackRange)
        {   
            Debug.Log("플레이어 감지!! dist : " + dist + "detectRange : " + detectRange );
            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetState(run: true);
            return;
        }

        // 🔹 공격 범위 진입
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            SetState(attack: true);
            Debug.Log("공격 애니메이션 재생?");
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
            Debug.Log($"NPC가 플레이어에게 {attackDamage} 데미지를 입힘!");
        }
    }
}
