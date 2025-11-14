using UnityEngine;
using UnityEngine.AI;

public interface IEnemyState
{


    void EnterState(EnemyStateManager enemy);

    void UpdateState(EnemyStateManager enemy);

    void ExitState(EnemyStateManager enemy);
}
