using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    Animator _anim;
    NavMeshAgent _agent;
    EnemyDetect _enemyDetect;
    EnemyDataManager _enemyDataMgr;
    EnemyData _enemyData;

    // Tuning
    float _loseSightGrace = 0.75f; // 감지 끊긴 뒤 허용 시간
    float _loseTimer = 0f; 
    float _keepDistance = 1.5f;    // 유지하고 싶은 간격
    float _tol = 0.1f;             // 근접 판정 여유

    // Repath control
    Vector3 _lastDest; 
    float _repathCooldown = 0.15f; // 6~10Hz 정도로만 경로 타임을 갱신
    float _repathTimer = 0f; // 경로 계산 타이머

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();
        _enemyDetect = enemy.GetComponent<EnemyDetect>();
        _enemyDataMgr = enemy.GetComponent<EnemyDataManager>();
        _enemyData = _enemyDataMgr._enemyData;

        _loseTimer = 0f; // 추적 타이머를 0으로 초기화
        _repathTimer = 0f; // 재탐색 타이머를 0으로 초기화
        _lastDest = enemy.transform.position; // 플레이어의 포지션을 _lastDest에 지정

        if (_agent != null)
        {
            _agent.enabled = true; // 에이전트 활성화
            _agent.isStopped = false; // Stop 꺼
            _agent.speed = _enemyData.ChaseSpeed;
            _agent.updatePosition = true; 
            _agent.updateRotation = true;
            _agent.stoppingDistance = _keepDistance; // 플레이어와의 거리를 최대 0.1까지 
            _agent.angularSpeed = Mathf.Max(_agent.angularSpeed, 120f);
            _agent.acceleration = Mathf.Max(_agent.acceleration, 8f);
            _agent.autoRepath = true; // 자동 재탐색 기능 활성화
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

        _repathTimer -= Time.deltaTime;

        var targetPos = _enemyDetect.Target.position;

        // NavMesh 위로 목적지 스냅(타깃이 비네비일 경우 대비), 에이전트 areaMask 사용
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
            enemy.TransitionToState(new MeleeState());
        }

        // 진단 로그: 필요 시 활성화
        // Debug.Log($"[Chase] stopped={_agent.isStopped}, hasPath={_agent.hasPath}, pending={_agent.pathPending}, status={_agent.pathStatus}, rem={_agent.remainingDistance:F2}, vel={_agent.velocity.magnitude:F2}");
    }
}

