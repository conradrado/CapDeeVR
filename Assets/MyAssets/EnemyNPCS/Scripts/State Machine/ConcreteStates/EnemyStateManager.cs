using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class EnemyStateManager : MonoBehaviour {

    public IEnemyState CurrentState;

    void Start()
    {
        TransitionToState(new PatrolState());
    }

    void Update()
    {
        CurrentState?.UpdateState(this);

    }


    public void TransitionToState(IEnemyState newState)
    {
        CurrentState?.ExitState(this);
        CurrentState = newState;
        CurrentState.EnterState(this);
        UnityEngine.Debug.Log($"[TransitionToState] State transitioned to {newState}");
    }



}
