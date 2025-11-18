using UnityEngine;

public class IdleState : IEnemyState
{
    float _idleTime;
    float _timer;
    Animator _anim;

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        if (_anim != null)
        {
            _anim.Play("Idle");
            _anim.SetBool("IsIdle", true);
            _anim.SetBool("IsChasing", false);
            _anim.SetBool("IsWalking", false);
            _anim.ResetTrigger("Melee");
        }
        _idleTime = Random.Range(1f, 4f);
        _timer = 0f;
    }

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Idle State] : State Exited");
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        _timer += Time.deltaTime;
        if (_timer >= _idleTime)
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }
    }
}
