using UnityEngine;

public class EnemyDetect : MonoBehaviour
{
    EnemyDataManager dataManager;
    [SerializeField] EnemyData _enemyData;
    [SerializeField] Transform _player; // Inspector 주입 권장

    public Transform Target { get; private set; }

    void Start()
    {
        ResolveDataIfNeeded();
        ResolveTargetIfNeeded();
    }

    bool _warnedMissingTarget;
    bool _warnedMissingData;

    void ResolveDataIfNeeded()
    {
        if (_enemyData != null)
            return;

        dataManager = GetComponent<EnemyDataManager>();
        if (dataManager != null)
            _enemyData = dataManager._enemyData;

        if (_enemyData == null && !_warnedMissingData)
        {
            Debug.LogWarning("[EnemyDetect] EnemyData is null. Detection will be skipped.");
            _warnedMissingData = true;
        }
    }

    void ResolveTargetIfNeeded()
    {
        if (Target != null)
            return;

        if (_player != null)
            Target = _player;

        if (Target == null)
        {
            var go = GameObject.FindWithTag("Player");
            Target = go != null ? go.transform : null;
        }

        if (Target == null && !_warnedMissingTarget)
        {
            Debug.LogWarning("[EnemyDetect] Player target not found. Will retry.");
            _warnedMissingTarget = true;
        }
    }

    bool HasValidData()
    {
        ResolveTargetIfNeeded();
        ResolveDataIfNeeded();
        return Target != null && _enemyData != null;
    }

    public bool IsPlayerInDetectRange()
    {
        if (!HasValidData())
            return false;

        float chaseRange = Mathf.Max(0f, _enemyData.DetectRange);
        return Vector3.Distance(Target.position, transform.position) <= chaseRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (!HasValidData())
            return false;

        float attackRange = Mathf.Max(0f, _enemyData.AttackRange);
        return Vector3.Distance(Target.position, transform.position) <= attackRange;
    }
}
