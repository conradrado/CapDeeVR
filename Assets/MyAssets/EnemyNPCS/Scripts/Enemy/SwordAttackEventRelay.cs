using UnityEngine;

/// <summary>
/// Lives on the same GameObject as the Animator so that animation events
/// can reach the actual SwordAttack component on the weapon child.
/// </summary>
public class SwordAttackEventRelay : MonoBehaviour
{
    [SerializeField] SwordAttack _swordAttack;

    void Reset()
    {
        if (_swordAttack == null)
            _swordAttack = GetComponentInChildren<SwordAttack>();
    }

    /// <summary>
    /// Animation Event hook (select this from the clip).
    /// </summary>
    public void OnSwordHitEvent()
    {
        if (_swordAttack == null)
        {
            Debug.LogWarning("[SwordAttackEventRelay] SwordAttack not assigned.", this);
            return;
        }

        _swordAttack.OnSwordHitEvent();
    }
}
