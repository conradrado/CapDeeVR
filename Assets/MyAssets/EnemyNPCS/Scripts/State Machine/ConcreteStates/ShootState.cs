using UnityEngine;
using UnityEngine.AI;

public class ShootState : IEnemyState
{
    const float MinShootCooldown = 0.15f;
    const float DefaultShootCooldown = 1.25f;
    const float DefaultShootRange = 10f;
    const float DefaultMeleeRange = 2f;
    const float DefaultDamage = 8f;
    const float LoseSightGrace = 1.25f;
    const float TurnSpeedDegrees = 540f;
    const float AimHeight = 1.5f;

    static readonly string[] AimKeywords = { "muzzle", "barrel", "gun", "fire", "shoot", "weapon" };

    Animator _anim;
    NavMeshAgent _agent;
    EnemyDetect _enemyDetect;
    EnemyData _enemyData;
    bool _isRangedOnly;

    Transform _target;
    IDamageable _damageableTarget;
    Transform _aimOrigin;
    IEnemyRangedShooter _customShooter;

    float _shootCooldown;
    float _shootTimer;
    float _shootRange;
    float _meleeSwitchRange;
    float _chaseSwitchRange;
    float _loseSightTimer;
    LayerMask _losMask = Physics.DefaultRaycastLayers;

    bool _cachedAgentRotation;
    bool _hasAgentRotationCache;

    public void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("[Shoot State] : State Entered");

        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();
        _enemyDetect = enemy.GetComponent<EnemyDetect>();

        var dataManager = enemy.GetComponent<EnemyDataManager>();
        _enemyData = dataManager != null ? dataManager._enemyData : null;
        _isRangedOnly = _enemyData != null && _enemyData.IsRangedOnly;

        RefreshTarget();
        if (_target != null)
            _target.TryGetComponent(out _damageableTarget);

        _shootCooldown = ResolveShootCooldown();
        _shootTimer = 0f;
        _loseSightTimer = 0f;
        _meleeSwitchRange = DetermineMeleeRange();
        _shootRange = DetermineShootRange();
        _chaseSwitchRange = _shootRange + 2.5f;

        CacheAimOrigin(enemy.transform);
        CacheShooter(enemy);

        StopAgentForShooting();
        UpdateAnimatorForShoot();
        AimAtTarget(enemy.transform);
    }

    public void ExitState(EnemyStateManager enemy)
    {
        ResumeAgent();
        if (_anim != null)
        {
            _anim.ResetTrigger("Shoot");
            _anim.SetBool("IsWalking", false);
        }
        Debug.Log("[Shoot State] : State Exited");
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        RefreshTarget();
        if (!HasValidTarget())
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }

        var self = enemy.transform;
        var targetPos = _target.position;
        float distance = Vector3.Distance(self.position, targetPos);

        bool inDetect = _enemyDetect != null && _enemyDetect.HasTarget;
        bool withinShootRange = distance <= _shootRange;

        if (_isRangedOnly)
        {
            if (!inDetect || !withinShootRange)
            {
                enemy.TransitionToState(new PatrolState());
                return;
            }
        }
        else if (distance <= _meleeSwitchRange)
        {
            enemy.TransitionToState(new MeleeState());
            return;
        }

        bool withinShoot = distance <= _chaseSwitchRange && inDetect;
        if (!_isRangedOnly && !withinShoot)
        {
            enemy.TransitionToState(new ChaseState());
            return;
        }

        bool hasLine = HasLineOfSight(self.position, targetPos);
        if (hasLine)
        {
            _loseSightTimer = 0f;
        }
        else
        {
            _loseSightTimer += Time.deltaTime;
            if (_loseSightTimer >= LoseSightGrace)
            {
                enemy.TransitionToState(_isRangedOnly ? new PatrolState() : new ChaseState());
                return;
            }
        }

        KeepAgentStopped();
        AimAtTarget(self);
        UpdateAimOrigin(self);

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f && hasLine)
        {
            TriggerShot();
            _shootTimer = _shootCooldown;
        }
    }

    float ResolveShootCooldown()
    {
        float baseCooldown = _enemyData != null && _enemyData.AttackSpeed > 0f
            ? _enemyData.AttackSpeed
            : DefaultShootCooldown;
        return Mathf.Max(MinShootCooldown, baseCooldown);
    }

    float DetermineMeleeRange()
    {
        float attack = _enemyData != null && _enemyData.AttackRange > 0f ? _enemyData.AttackRange : DefaultMeleeRange;
        return attack + 0.35f;
    }

    float DetermineShootRange()
    {
        float detect = _enemyData != null && _enemyData.DetectRange > 0f ? _enemyData.DetectRange : DefaultShootRange;
        float attack = _enemyData != null && _enemyData.AttackRange > 0f ? _enemyData.AttackRange : DefaultMeleeRange;
        float agentStop = _agent != null ? Mathf.Max(0.5f, _agent.stoppingDistance) : 0f;
        return Mathf.Max(detect, attack + 4f, agentStop + 3.5f, DefaultShootRange);
    }

    bool HasValidTarget()
    {
        return _target != null && _damageableTarget != null;
    }

    void StopAgentForShooting()
    {
        if (_agent == null || !_agent.enabled)
            return;

        if (!_hasAgentRotationCache)
        {
            _cachedAgentRotation = _agent.updateRotation;
            _hasAgentRotationCache = true;
        }

        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _agent.updateRotation = false;
    }

    void KeepAgentStopped()
    {
        if (_agent == null || !_agent.enabled)
            return;

        if (!_agent.isStopped)
            _agent.isStopped = true;

        _agent.velocity = Vector3.zero;
    }

    void ResumeAgent()
    {
        if (_agent == null || !_agent.enabled)
            return;

        _agent.isStopped = false;
        if (_hasAgentRotationCache)
            _agent.updateRotation = _cachedAgentRotation;
    }

    void UpdateAnimatorForShoot()
    {
        if (_anim == null)
            return;

        _anim.applyRootMotion = false;
        _anim.SetBool("IsWalking", false);
        _anim.SetBool("IsChasing", false);
        _anim.SetFloat("Speed", 0f);
        _anim.ResetTrigger("Melee");
        _anim.ResetTrigger("Shoot");
    }

    void AimAtTarget(Transform self)
    {
        if (_target == null)
            return;

        var lookDir = _target.position - self.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.0001f)
            return;

        var targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        self.rotation = Quaternion.RotateTowards(self.rotation, targetRot, TurnSpeedDegrees * Time.deltaTime);
    }

    bool HasLineOfSight(Vector3 selfPos, Vector3 targetPos)
    {
        Vector3 origin = _aimOrigin != null ? _aimOrigin.position : selfPos + Vector3.up * AimHeight;
        Vector3 destination = targetPos + Vector3.up * AimHeight;
        Vector3 toTarget = destination - origin;
        float distance = toTarget.magnitude;

        if (distance <= Mathf.Epsilon)
            return true;

        if (Physics.Raycast(origin, toTarget.normalized, out var hit, distance, _losMask, QueryTriggerInteraction.Ignore))
        {
            if (_target == null)
                return false;

            if (hit.transform == _target || hit.transform.IsChildOf(_target))
                return true;

            return false;
        }

        return true;
    }

    void TriggerShot()
    {
        if (_anim != null)
        {
            _anim.ResetTrigger("Shoot");
            _anim.SetTrigger("Shoot");
        }

        if (_target == null)
            return;

        float damage = ResolveDamage();

        if (_customShooter != null)
        {
            _customShooter.ShootAt(_target, damage);
        }
        else
        {
            ApplyHitscanDamage(damage);
        }
    }

    void ApplyHitscanDamage(float damage)
    {
        if (_damageableTarget == null)
            return;

        _damageableTarget.TakeDamage(damage);
    }

    float ResolveDamage()
    {
        if (_enemyData != null && _enemyData.Damage > 0f)
            return _enemyData.Damage;
        return DefaultDamage;
    }

    void CacheAimOrigin(Transform root)
    {
        if (root == null)
            return;

        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root)
                continue;

            string lower = t.name.ToLowerInvariant();
            foreach (var keyword in AimKeywords)
            {
                if (lower.Contains(keyword))
                {
                    _aimOrigin = t;
                    return;
                }
            }
        }
    }

    void UpdateAimOrigin(Transform root)
    {
        if (_aimOrigin != null)
            return;

        CacheAimOrigin(root);
    }

    void CacheShooter(EnemyStateManager enemy)
    {
        if (enemy == null)
            return;

        foreach (var mono in enemy.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mono is IEnemyRangedShooter shooter)
            {
                _customShooter = shooter;
                return;
            }
        }
    }

    void RefreshTarget()
    {
        if (_enemyDetect == null)
            return;

        _enemyDetect.RefreshTarget();
        _target = _enemyDetect.Target;
        _damageableTarget = null;
        if (_target != null)
            _target.TryGetComponent(out _damageableTarget);
    }
}

/// <summary>
/// Optional component hook that lets the state delegate projectile spawning.
/// </summary>
public interface IEnemyRangedShooter
{
    void ShootAt(Transform target, float damage);
}
