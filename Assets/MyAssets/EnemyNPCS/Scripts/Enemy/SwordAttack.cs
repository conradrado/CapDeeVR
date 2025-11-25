using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Tooltip("Layers that can be damaged by the sword.")]
    [SerializeField] LayerMask _targetLayers = ~0;
    [Tooltip("Damage used when EnemyData is missing.")]
    [SerializeField] float _fallbackDamage = 10f;

    BoxCollider _hitbox;
    EnemyDetect _enemyDetect;
    EnemyDataManager _dataManager;
    IDamageable _cachedTarget;
    Transform _cachedTransform;

    void Awake()
    {
        _hitbox = GetComponent<BoxCollider>();
        if (_hitbox == null)
        {
            Debug.LogWarning("[SwordAttack] BoxCollider missing. Overlap hit test will fail.", this);
        }

        _enemyDetect = GetComponentInParent<EnemyDetect>();
        _dataManager = GetComponentInParent<EnemyDataManager>();
    }

    void Start()
    {
        CachePlayerStat();
    }

    public void CachePlayerStat()
    {
        _cachedTarget = null;
        _cachedTransform = null;

        if (_enemyDetect != null && _enemyDetect.Target != null)
        {
            _cachedTransform = _enemyDetect.Target;
            _enemyDetect.Target.TryGetComponent(out _cachedTarget);
        }

        if (_cachedTarget == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _cachedTransform = player.transform;
                player.TryGetComponent(out _cachedTarget);
            }
        }
    }

    public float ResolveDamage() =>
        _dataManager != null && _dataManager._enemyData != null
            ? _dataManager._enemyData.Damage
            : _fallbackDamage;

    /// <summary>
    /// Animation event entry point. Call this on the contact frame of the sword swing.
    /// </summary>
    public void OnSwordHitEvent()
    {
        CachePlayerStat();
        if (_cachedTarget == null)
        {
            Debug.LogWarning("[SwordAttack] No damageable target cached, cannot apply damage.", this);
            return;
        }

        if (!TryHitTarget(out var target))
        {
            if (_enemyDetect == null || !_enemyDetect.IsCurrentTargetInAttackRange())
                return;

            target = _cachedTarget;
        }

        target.TakeDamage(ResolveDamage());
    }

    bool TryHitTarget(out IDamageable hitTarget)
    {
        hitTarget = null;
        if (_hitbox == null)
            return false;

        var center = _hitbox.transform.TransformPoint(_hitbox.center);
        var halfExtents = Vector3.Scale(_hitbox.size * 0.5f, _hitbox.transform.lossyScale);
        var orientation = _hitbox.transform.rotation;

        var hits = Physics.OverlapBox(center, halfExtents, orientation, _targetLayers, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target))
                continue;

            if (_cachedTransform != null && hit.transform != _cachedTransform)
                continue;

            hitTarget = target;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_hitbox == null)
            _hitbox = GetComponent<BoxCollider>();
        if (_hitbox == null)
            return;

        Gizmos.color = Color.red;
        var center = _hitbox.transform.TransformPoint(_hitbox.center);
        var size = Vector3.Scale(_hitbox.size, _hitbox.transform.lossyScale);
        var matrix = Matrix4x4.TRS(center, _hitbox.transform.rotation, size);
        Gizmos.matrix = matrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
#endif

}
