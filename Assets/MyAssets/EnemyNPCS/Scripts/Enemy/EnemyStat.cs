using UnityEngine;


public class EnemyStat : StatBehaviour
{
    [Header("Data")]
    [SerializeField] EnemyDataManager _enemyDataMgr;
    [SerializeField] EnemyData _enemyData;

    [Header("UI")]
    [SerializeField] HPBar _hpBar;

    [Header("FX")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _healSound;

    EnemyStateManager _stateManager;
    bool _isDead;

    public float Damage { get; private set; }

    void Awake()
    {
        _stateManager = GetComponent<EnemyStateManager>();
        InitializeData();
        InitializeHp();
    }

    void InitializeData()
    {
        if (_enemyData == null && _enemyDataMgr != null)
            _enemyData = _enemyDataMgr._enemyData;

        if (_enemyData != null)
        {
            MaxHP = _enemyData.MaxHP;
            Damage = _enemyData.Damage;
        }
        else
        {
            Debug.LogWarning("[EnemyStat] EnemyData is not assigned. Using inspector values.", this);
        }
    }

    void InitializeHp()
    {
        CurrentHP = Mathf.Max(0f, MaxHP);

        if (_hpBar != null)
        {
            _hpBar.SetMaxHP(MaxHP);
            _hpBar.SetHP(MaxHP, CurrentHP);
        }
    }

    public override void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        if (_stateManager != null)
        {
            _stateManager.TransitionToState(new DeathState());
        }
    }

    public override void HealDamage(float healAmount)
    {
        if (_isDead || CurrentHP >= MaxHP)
            return;

        CurrentHP = Mathf.Min(CurrentHP + healAmount, MaxHP);
        if (_hpBar != null)
            _hpBar.SetHP(MaxHP, CurrentHP);

        if (_audioSource != null && _healSound != null)
            _audioSource.PlayOneShot(_healSound);
    }

    public override void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        CurrentHP = Mathf.Max(0f, CurrentHP - damage);

        if (_hpBar != null)
            _hpBar.SetHP(MaxHP, CurrentHP);

        if (CurrentHP <= 0f)
        {
            Die();
        }
    }
}