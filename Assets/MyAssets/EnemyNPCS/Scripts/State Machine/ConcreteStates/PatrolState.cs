using UnityEngine;
using UnityEngine.AI;


public class PatrolState : IEnemyState
{
    NavMeshAgent _agent;
    Animator _anim;
    Vector3 _patrolCenter;
    EnemyDetect _enemyDetect;
    EnemyDataManager _enemyDataMgr;
    EnemyData _enemyData;
    bool _isRangedOnly;
    float _patrolRadius = 10f;

    float _moveTime = 0f;
    float _moveTimer = 0f;

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();
        _enemyDataMgr = enemy.GetComponent<EnemyDataManager>();
        _enemyData = _enemyDataMgr != null ? _enemyDataMgr._enemyData : null;
        _isRangedOnly = _enemyData != null && _enemyData.IsRangedOnly;

        _patrolCenter = enemy.transform.position;

        if (_anim != null)
        {
            _anim.SetBool("IsIdle", false);
            _anim.SetBool("IsChasing", false);
            _anim.SetBool("IsWalking", true);
        }

        if (_agent != null)
        {
            _agent.isStopped = false;
            _agent.autoBraking = true;
        }

        _enemyDetect = enemy.GetComponent<EnemyDetect>();
    }   

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Patrol State] : State Exited");
        if (_anim != null)
        {
            _anim.SetBool("IsWalking", false);
            _anim.SetFloat("Speed", 0f);
        }
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        _enemyDetect.RefreshTarget();

        if (_anim != null && _agent != null)
            _anim.SetFloat("Speed", _agent.velocity.magnitude);

        if (_agent != null && (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance + 0.1f))
        {
            if (_moveTime <= 0f)
            {
                _moveTime = Random.Range(1f, 4f);
                _moveTimer = 0f;
            }

            _moveTimer += Time.deltaTime;
            if (_moveTimer >= _moveTime)
            {
                _moveTime = 0f; // reset next wander time
                PickNewDestination(_patrolCenter);
            }
        }

        if (_enemyDetect.HasTarget)
        {
            enemy.TransitionToState(_isRangedOnly ? new ShootState() : new ChaseState());
        }
    }
    
    private void PickNewDestination(Vector3 center)
    {
        if (TryGetRandomPointOnNavMesh(center, _patrolRadius,out var point))
        {   
            _agent.SetDestination(point);
        }
    }

    private bool TryGetRandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            var random = Random.insideUnitSphere * radius;
            random.y = 0f;
            var candidate = center + random;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }
}
