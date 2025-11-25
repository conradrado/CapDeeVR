using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    Animator _anim;
    NavMeshAgent _agent;
    EnemyDetect _enemyDetect;
    EnemyDataManager _enemyDataMgr;
    EnemyData _enemyData;
    bool _isRangedOnly;

    // Tuning
    float _loseSightGrace = 0.75f; // 감시를 잃은 후 유지 시간
    float _loseTimer = 0f;
    float _keepDistance = 1.5f;    // 목표와의 최소 간격
    float _tol = 0.1f;             // 근접 판정 여유

    // Repath control
    Vector3 _lastDest;
    float _repathCooldown = 0.15f; // 6~10Hz 빈도로만 경로 요청
    float _repathTimer = 0f; // 경로 계산 타이머

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();
        _enemyDetect = enemy.GetComponent<EnemyDetect>();
        _enemyDataMgr = enemy.GetComponent<EnemyDataManager>();
        _enemyData = _enemyDataMgr != null ? _enemyDataMgr._enemyData : null;
        _isRangedOnly = _enemyData != null && _enemyData.IsRangedOnly;

        _loseTimer = 0f;
        _repathTimer = 0f;
        _lastDest = enemy.transform.position;
        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.speed = _enemyData != null ? _enemyData.ChaseSpeed : _agent.speed;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.stoppingDistance = _keepDistance;
            _agent.angularSpeed = Mathf.Max(_agent.angularSpeed, 120f);
            _agent.acceleration = Mathf.Max(_agent.acceleration, 8f);
            _agent.autoRepath = true;
            _agent.autoBraking = false;
        }

        if (_anim != null)
        {
            _anim.applyRootMotion = false; // NavMeshAgent와 충돌 방지
            _anim.SetBool("IsWalking", false);
            _anim.SetBool("IsChasing", true);
            _anim.SetBool("Melee", false);
        }

        Debug.Log("[Chase State] : State Entered");
    }

    public void ExitState(EnemyStateManager enemy)
    {
        if (_anim != null) _anim.SetBool("IsChasing", false);
        if (_agent != null) _agent.isStopped = false;
        Debug.Log("[Chase State] : State Exited");
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        if (_agent == null)
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }
        if (_enemyDetect == null || _enemyDetect.Target == null)
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }

        if (_isRangedOnly)
        {
            if (_enemyDetect.IsPlayerInDetectRange())
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                enemy.TransitionToState(new ShootState());
            }
            else
            {
                enemy.TransitionToState(new PatrolState());
            }
            return;
        }

        _repathTimer -= Time.deltaTime;

        var targetPos = _enemyDetect.Target.position;

        // NavMesh 위의 유효 목적지 추출
        if (NavMesh.SamplePosition(targetPos, out var hit, 2f, _agent.areaMask))
            targetPos = hit.position;

        var selfPos = enemy.transform.position;
        float dist = Vector3.Distance(selfPos, targetPos);

        if (_enemyDetect.IsPlayerInDetectRange())
        {
            _loseTimer = 0f;

            if (dist > _agent.stoppingDistance + _tol)
            {
                if (!_agent.pathPending)
                {
                    bool needRepath = _repathTimer <= 0f &&
                        (Vector3.SqrMagnitude(_lastDest - targetPos) > 0.25f || _agent.pathStatus != NavMeshPathStatus.PathComplete);

                    if (needRepath)
                    {
                        var path = new NavMeshPath();
                        bool ok = NavMesh.CalculatePath(selfPos, targetPos, _agent.areaMask, path) && path.status != NavMeshPathStatus.PathInvalid;
                        if (ok)
                        {
                            var final = path.corners != null && path.corners.Length > 0 ? path.corners[path.corners.Length - 1] : targetPos;
                            _agent.isStopped = false;
                            _agent.SetDestination(final);
                            _lastDest = final;
                            _repathTimer = _repathCooldown;
                        }
                    }
                }
            }
            else
            {
                _agent.isStopped = true;
                var look = new Vector3(targetPos.x, selfPos.y, targetPos.z);
                enemy.transform.LookAt(look);
            }
        }
        else
        {
            _loseTimer += Time.deltaTime;
            if (_loseTimer >= _loseSightGrace)
            {
                enemy.TransitionToState(new PatrolState());
                return;
            }

            if (!_agent.pathPending && _repathTimer <= 0f)
            {
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(selfPos, targetPos, _agent.areaMask, path) && path.status != NavMeshPathStatus.PathInvalid)
                {
                    var final = path.corners != null && path.corners.Length > 0 ? path.corners[path.corners.Length - 1] : targetPos;
                    _agent.isStopped = false;
                    _agent.SetDestination(final);
                    _lastDest = final;
                    _repathTimer = _repathCooldown;
                }
            }
        }

        if (_enemyDetect.IsPlayerInAttackRange())
        {
            enemy.TransitionToState(_isRangedOnly ? new ShootState() : new MeleeState());
        }
    }
}
