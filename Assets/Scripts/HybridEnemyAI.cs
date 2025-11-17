using UnityEngine;
using UnityEngine.AI;

// Hybrid AI: Shoots at range, melee when close
public class HybridEnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public TargetEntity targetEntity; // Player health component

    [Header("Ranges")]
    public float detectRange = 15f;
    public float shootRange = 10f;
    public float meleeRange = 2f;

    [Header("Combat")]
    public float shootCooldown = 1.2f;
    public float meleeCooldown = 1.0f;
    public float turnSpeed = 10f;

    [Header("Shooting")]
    public Transform muzzle;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float bulletLife = 5f;
    public float shootDamage = 8f;
    public LayerMask losMask = ~0; // Line of sight mask

    [Header("Melee")]
    public float meleeDamage = 12f;

    [Header("Animator Params")]
    public string speedParam = "Speed";
    public string shootParam = "Shoot";
    public string meleeParam = "Melee";
    public string dieParam = "Die";

    private NavMeshAgent agent;
    private Animator anim;
    private bool isDead = false;
    private float nextShootTime = 0f;
    private float nextMeleeTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (player != null && targetEntity == null)
            targetEntity = player.GetComponent<TargetEntity>();

        if (agent != null)
            agent.stoppingDistance = Mathf.Max(0.1f, meleeRange * 0.8f);
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > detectRange)
        {
            Debug.Log("플레이어 감지 종료. 정지");
            StopMove();
            SetSpeed(0f);
            return;
        }

        FaceTarget();

        if (dist <= meleeRange && Time.time >= nextMeleeTime)
        {
            
            DoMelee();
            return;
        }

        if (dist <= shootRange && Time.time >= nextShootTime && HasLineOfSight())
        {
            DoShoot();
            return;
        }

        ResumeMove();
        if (agent != null)
        {
            agent.SetDestination(player.position);
            SetSpeed(agent.velocity.magnitude);
        }
    }

    void FaceTarget()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }

    bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 target = player.position + Vector3.up * 1.6f;
        Vector3 to = target - origin;
        return !Physics.Raycast(origin, to.normalized, to.magnitude, losMask, QueryTriggerInteraction.Ignore);
    }

    void DoShoot()
    {
        StopMove();
        SetSpeed(0f);
        if (anim != null)
        {
            anim.ResetTrigger(meleeParam);
            anim.SetTrigger(shootParam);
        }
        nextShootTime = Time.time + shootCooldown;
    }

    void DoMelee()
    {
        StopMove();
        SetSpeed(0f);
        if (anim != null)
        {
            anim.ResetTrigger(shootParam);
            anim.SetTrigger(meleeParam);
        }
        nextMeleeTime = Time.time + meleeCooldown;
    }

    void StopMove()
    {
        if (agent != null) agent.isStopped = true;
    }

    void ResumeMove()
    {
        if (agent != null) agent.isStopped = false;
    }

    void SetSpeed(float s)
    {
        if (anim != null) anim.SetFloat(speedParam, s);
    }

    // Animation event: called at the muzzle flash frame
    public void FireProjectile()
    {
        if (muzzle == null || bulletPrefab == null) return;
        GameObject go = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        if (go.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = muzzle.forward * bulletSpeed;
        if (go.TryGetComponent<SimpleBullet>(out var bullet))
            bullet.damage = shootDamage;
        Destroy(go, bulletLife);
    }

    // Animation event: called on the hit frame of the melee animation
    public void DealMeleeDamage()
    {
        if (targetEntity == null || player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= meleeRange + 0.3f)
            targetEntity.TakeDamage(meleeDamage);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopMove();
        if (anim != null) anim.SetBool(dieParam, true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, shootRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}

