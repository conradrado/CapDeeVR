using UnityEngine;
using UnityEngine.AI;


public class DeathState : IEnemyState
{
    const float FallbackDestroyDelay = 3f; // If we cannot read an actual death clip length

    Animator _anim;
    NavMeshAgent _agent;
    float _destroyTimer;
    bool _destroyQueued;

    public void EnterState(EnemyStateManager enemy)
    {
        _anim = enemy.GetComponent<Animator>();
        _agent = enemy.GetComponent<NavMeshAgent>();

        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        float destroyDelay = ResolveDeathClipLength();
        if (_anim != null)
        {
            // Stop any locomotion blend states and fire the death animation.
            _anim.SetBool("IsIdle", false);
            _anim.SetBool("IsChasing", false);
            _anim.SetBool("IsWalking", false);
            _anim.ResetTrigger("Melee");
            _anim.ResetTrigger("Shoot");

            // The controller is expected to have a death trigger/state named "Die" or "Death".
            PlayDeathAnimation();
            if (destroyDelay <= 0f)
            {
                destroyDelay = FallbackDestroyDelay;
            }
        }
        else
        {
            destroyDelay = FallbackDestroyDelay;
        }

        _destroyTimer = destroyDelay;
        Debug.Log($"[Death State] : Entered. Destroying after {_destroyTimer:F2}s");
    }

    public void ExitState(EnemyStateManager enemy)
    {
        Debug.Log("[Death State] : State Exited");
    }

    public void UpdateState(EnemyStateManager enemy)
    {
        if (_destroyQueued)
            return;

        _destroyTimer -= Time.deltaTime;
        if (_destroyTimer <= 0f)
        {
            _destroyQueued = true;
            Object.Destroy(enemy.gameObject);
        }
    }

    float ResolveDeathClipLength()
    {
        if (_anim == null || _anim.runtimeAnimatorController == null)
            return 0f;

        foreach (var clip in _anim.runtimeAnimatorController.animationClips)
        {
            string lower = clip.name.ToLowerInvariant();
            if (lower.Contains("death") || lower.Contains("die"))
                return clip.length;
        }

        return 0f;
    }

    void PlayDeathAnimation()
    {
        if (_anim == null)
            return;

        _anim.SetBool("IsDead", true);
        _anim.Play("Death", 0, 0f); // Fallback to a plainly named state if parameters are missing.
    }

    bool HasParameter(string name, AnimatorControllerParameterType type)
    {
        if (_anim == null)
            return false;

        foreach (var param in _anim.parameters)
        {
            if (param.type == type && param.name == name)
                return true;
        }
        return false;
    }
}
