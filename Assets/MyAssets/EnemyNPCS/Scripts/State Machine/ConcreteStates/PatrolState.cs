using TMPro;
using UnityEditor;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;


public class PatrolState : IEnemyState
{

    NavMeshAgent _agent;
    Animator _anim;
    Vector3 _patrolCenter;
    EnemyDetect _enemyDetect;
    float _patrolRadius = 10f;


    float _moveTime = 0f;
    float _moveTimer = 0f;

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();

        _patrolCenter = enemy.transform.position;

        _anim.SetBool("IsChasing", false);
        _anim.SetBool("IsWalking", true);


        _agent.isStopped = false;
        _agent.autoBraking = true;
        _enemyDetect = enemy.GetComponent<EnemyDetect>();
    }   

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Patrol State] : State Exited");
        _anim.SetBool("IsWalking", false);
        _anim.SetFloat("Speed", 0f);

    }

    public void UpdateState(EnemyStateManager enemy)
    {


        // agent.hasPath => 에이전트가 갈 경로가 존재하다는 뜻.
        // agent.remainingDistance => 에이전트가 목적지까지 이동해야 할 남은 거리
        // agent.remainingDistance <= agent.stoppingDistance 는 agent가 목적지까지 거의 다 왔음을 의미

        _anim.SetFloat("Speed", _agent.velocity.magnitude);

        if (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            // 도착 후 랜덤 대기
            if (_moveTime <= 0f)
            {
                _moveTime = Random.Range(1f, 4f);
                _moveTimer = 0f;
            }

            _moveTimer += Time.deltaTime;
            if (_moveTimer >= _moveTime)
            {
                _moveTime = 0f; // 다음 도착 때 다시 설정
                PickNewDestination(_patrolCenter);
            }
        }

        if (_enemyDetect.IsPlayerInDetectRange())
        {
            enemy.TransitionToState(new ChaseState());
        }





    }
    
    private void PickNewDestination(Vector3 center)
    {
        if(TryGetRandomPointOnNavMesh(center, _patrolRadius,out var point))
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