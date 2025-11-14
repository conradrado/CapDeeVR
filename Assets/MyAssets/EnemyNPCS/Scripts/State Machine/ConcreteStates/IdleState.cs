using UnityEngine;
using UnityEngine.AI;


public class IdleState : IEnemyState
{
    float _idleTime; // Idle하는 시간
    float _timer; // 타이머
    Animator _anim;

    public void EnterState(EnemyStateManager enemy)
    {
        enemy.GetComponent<Animator>().Play("Idle"); // 애니메이터를 get한 뒤, "Idle" 애니메이션 실행
        _idleTime = Random.Range(1f, 4f);
        _timer = 0f;
        _anim.SetBool("IsIdle", true);
        _anim.SetBool("IsChasing", false);
        _anim.SetBool("IsWalking", false);
        _anim.ResetTrigger("Melee");
    }

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Idle State] : State Exited");
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        _timer += Time.deltaTime;
        if(_timer >= _idleTime)
        {
            enemy.TransitionToState(new PatrolState());
            return;
        }    
    }
}