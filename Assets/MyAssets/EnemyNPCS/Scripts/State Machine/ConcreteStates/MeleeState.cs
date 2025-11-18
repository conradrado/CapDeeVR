using UnityEngine;
using UnityEngine.AI;


public class MeleeState : IEnemyState
{
    Animator _anim;
    NavMeshAgent _agent;
    EnemyDetect _enemyDetect;
    EnemyData _enemyData;

    
    float _attackTimer;

    public void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("[Attack State] : State Entered");

        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();
        _enemyDetect = enemy.GetComponent<EnemyDetect>();
        var dataManager = enemy.GetComponent<EnemyDataManager>();


        if (_anim != null)
        {
            _anim.SetBool("IsChasing", false);
            _anim.SetBool("IsWalking", false);
            _anim.SetTrigger("Melee");
        }
        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.isStopped = true;
        }

        if (dataManager != null)
        {
            _enemyData = dataManager._enemyData;
        }
        else
        {
            _enemyData = null;
            Debug.LogWarning("[Melee State] EnemyDataManager component missing.");
        }


        if (_enemyData == null)
        {
            Debug.LogWarning("[Melee State] EnemyData is null. Attacks will be skipped.");
            _attackTimer = 0f;
        }
        else
        {
            _attackTimer = Mathf.Max(0.01f, _enemyData.AttackSpeed);
        }
    }

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Attack State] : State Entered");
        _agent.isStopped = false;
        _anim.ResetTrigger("Melee");

    }

    public void UpdateState(EnemyStateManager enemy)
    {
        if (_enemyDetect == null || _enemyData == null)
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }

        float attackSpeed = Mathf.Max(0.01f, _enemyData.AttackSpeed);


        if (_enemyDetect.IsPlayerInAttackRange())
        {

            Debug.Log("[Melee State] : Player is in melee range");

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                if (_anim != null)
                {
                    _anim.SetTrigger("Melee");
                }
                _attackTimer = attackSpeed;
            }
            
            
        }
        else if (_enemyDetect.IsPlayerInDetectRange())
        {
            _attackTimer = attackSpeed;
            if (_anim != null)
            {
                _anim.ResetTrigger("Melee");
            }
            enemy.TransitionToState(new ChaseState());
        }
        else
        {
            _attackTimer = attackSpeed;
            enemy.TransitionToState(new PatrolState());
        }
    }
}
