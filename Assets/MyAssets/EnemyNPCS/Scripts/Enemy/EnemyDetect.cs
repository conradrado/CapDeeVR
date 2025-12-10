using UnityEngine;

public class EnemyDetect : MonoBehaviour
{
    EnemyDataManager dataManager;
    [SerializeField] EnemyData _enemyData;
    [SerializeField] Transform _player; // Inspector 주입 권장
    [SerializeField] Transform _defendObject; // 지켜야 할 오브젝트

    public enum TargetType { None, Player, Object }

    public Transform Target => _currentTarget;
    public TargetType CurrentTargetType { get; private set; } = TargetType.None;
    public bool HasTarget => _currentTarget != null && CurrentTargetType != TargetType.None;
    public Transform DefendObject => _defendObject;

    public void SetDefendObject(Transform defendTarget)
    {
        _defendObject = defendTarget;
        RefreshTarget();
    }

    Transform _currentTarget;

    void Start()
    {
        ResolveDataIfNeeded();
        ResolveTargetIfNeeded();
        RefreshTarget();
    }

    void Update()
    {
        RefreshTarget();
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
        if (_player != null)
            return;

        if (Target == null)
        {
            var go = GameObject.FindWithTag("Player");
            _player = go != null ? go.transform : null;
        }

        if (_player == null && !_warnedMissingTarget)
        {
            Debug.LogWarning("[EnemyDetect] Player target not found. Will retry.");
            _warnedMissingTarget = true;
        }
    }

    bool HasValidData()
    {
        ResolveTargetIfNeeded();
        ResolveDataIfNeeded();
        return _enemyData != null;
    }

    public bool IsPlayerInDetectRange()
    {
        if (!HasValidData())
            return false;

        float chaseRange = Mathf.Max(0f, _enemyData.DetectRange);
        return _player != null && Vector3.Distance(_player.position, transform.position) <= chaseRange;
    }

    public bool IsObjectInDetectRange()
    {
        if (!HasValidData())
            return false;

        if (_defendObject == null)
            return false;

        float objRange = _enemyData.ObjectDetectRange > 0f
            ? _enemyData.ObjectDetectRange
            : Mathf.Max(0f, _enemyData.DetectRange);

        return Vector3.Distance(_defendObject.position, transform.position) <= objRange;
    }

    public bool IsCurrentTargetInAttackRange()
    {
        if (!HasValidData())
            return false;

        if (CurrentTargetType == TargetType.None || Target == null)
            return false;

        float attackRange = Mathf.Max(0f, _enemyData.AttackRange);
        return Vector3.Distance(Target.position, transform.position) <= attackRange;
    }

    public void RefreshTarget()
    {
        ResolveDataIfNeeded();
        ResolveTargetIfNeeded();

        bool playerInRange = IsPlayerInDetectRange();
        bool objectInRange = IsObjectInDetectRange();

        EnemyData.TargetPriority priority = _enemyData != null ? _enemyData.Priority : EnemyData.TargetPriority.PlayerFirst;

        if (playerInRange && objectInRange)
        {
            if (priority == EnemyData.TargetPriority.ObjectFirst)
                SetTarget(_defendObject, TargetType.Object);
            else
                SetTarget(_player, TargetType.Player);
        }
        else if (objectInRange)
        {
            SetTarget(_defendObject, TargetType.Object);
        }
        else if (playerInRange)
        {
            SetTarget(_player, TargetType.Player);
        }
        else
        {
            SetTarget(null, TargetType.None);
        }
    }

    void SetTarget(Transform newTarget, TargetType type)
    {
        _currentTarget = newTarget;
        CurrentTargetType = type;
    }
}
