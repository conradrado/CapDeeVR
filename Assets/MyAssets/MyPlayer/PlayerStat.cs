using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStat : StatBehaviour, IDamageable
{
    [Header("UI")]
    [SerializeField] HPBar _hpBar;

    [Header("FX")]
    [SerializeField] Volume damageVolume;
    [SerializeField] VolumeProfile normalProfile;
    [SerializeField] VolumeProfile damageProfile;
    [SerializeField] float damageFXDuration = 0.4f;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip healSound;

    [Header("Death")]
    [SerializeField] DeathManager deathManager;
    [SerializeField] bool pauseOnDeath = true;

    Coroutine damageFxCoroutine;
    bool _isDead;
    float _prevTimeScale = 1f;

    static int ammoBonus = 0;
    static float damageBonus = 0f;

    public static int AmmoBonus => ammoBonus;
    public static float DamageBonus => damageBonus;

    void Awake()
    {
        InitializeHp();
        SetupDamageVolume();
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

    public void AddDamageBonus(float dmgBonus)
    {
        damageBonus += dmgBonus;
    }

    public void AddAmmoBonus(int ammo)
    {
        ammoBonus += ammo;
    }

    public bool isFull()
    {
        return CurrentHP == MaxHP;
    }

    void SetupDamageVolume()
    {
        if (damageVolume == null)
            return;
        if (normalProfile == null)
            normalProfile = damageVolume.profile;
        damageVolume.weight = 0f;
    }

    public override void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        CurrentHP = Mathf.Max(0f, CurrentHP - damage);
        if (_hpBar != null)
            _hpBar.SetHP(MaxHP, CurrentHP);

        TriggerDamageFX();

        if (CurrentHP <= 0f)
            Die();
    }

    public override void HealDamage(float healAmount)
    {
        if (_isDead || CurrentHP >= MaxHP)
            return;

        CurrentHP = Mathf.Min(CurrentHP + healAmount, MaxHP);
        if (_hpBar != null)
            _hpBar.SetHP(MaxHP, CurrentHP);

        if (audioSource != null && healSound != null)
            audioSource.PlayOneShot(healSound);
    }

    public override void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        if (deathManager != null)
            deathManager.ShowDeathUI();

        if (pauseOnDeath)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    void TriggerDamageFX()
    {
        if (damageVolume == null || damageProfile == null)
            return;

        if (damageFxCoroutine != null)
            StopCoroutine(damageFxCoroutine);
        damageFxCoroutine = StartCoroutine(DamageFlashFX());
    }

    IEnumerator DamageFlashFX()
    {
        VolumeProfile prevProfile = damageVolume.profile;
        if (normalProfile == null)
            normalProfile = prevProfile;

        damageVolume.profile = damageProfile;
        damageVolume.weight = 1f;

        float t = 0f;
        while (t < damageFXDuration)
        {
            t += Time.unscaledDeltaTime;
            float w = 1f - Mathf.Clamp01(t / damageFXDuration);
            damageVolume.weight = w;
            yield return null;
        }

        damageVolume.profile = normalProfile;
        damageVolume.weight = 0f;
        damageFxCoroutine = null;
    }

    public bool IsFull() => CurrentHP >= MaxHP;

    void OnDisable()
    {
        // If we paused on death and get re-enabled, make sure time resumes.
        if (_isDead && pauseOnDeath && Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
        }
    }
}
