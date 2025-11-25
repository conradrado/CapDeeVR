using UnityEngine;
using UnityEngine.Rendering;

public class TargetEntity : MonoBehaviour, IDamageable
{

    [Header("스탯")]
    // 해당 엔티티, NPC의 최대 체력
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private HPBar hpBar;

    [Header("플레이어인가요?")]
    [SerializeField] private bool isPlayer = false;

    [Header("체력 회복 사운드 효과")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip healSound;
    
    [Header("플레이어의 죽음 처리 스크립트")]
    public DeathManager deathManager;

    [Header("Damage Screen Effect")]
    [SerializeField] private Volume damageVolume;
    [SerializeField] private VolumeProfile normalProfile;
    [SerializeField] private VolumeProfile damageProfile;
    [SerializeField] private float damageFXDuration = 0.4f;
    private Coroutine damageFxCoroutine;

    // 해당 엔티티, NPC의 현재 체력
    private float currentHP;
    private bool isDead = false;
    // Awake 함수는 Start 함수보다 먼저 실행됨
    private void Awake()
    {
        // 현재 체력 = 최대 체력
        currentHP = maxHP;
        hpBar.SetMaxHP(maxHP);
        if (damageVolume != null)
        {
            if (normalProfile == null)
                normalProfile = damageVolume.profile;
            damageVolume.weight = 0f;
        }
    }

    /// <summary>
    /// 총알 등에 의해 호출됨
    /// </summary>

    /// 데미지 처리 함수, amount는 데미지
    public void TakeDamage(float amount)
    {   
        // 현재 체력에서 amount 만큼 차감
        currentHP -= amount;
        Debug.Log("으악! HP : " + currentHP);
        hpBar.SetHP(maxHP, currentHP); 
        if (isPlayer)
            TriggerDamageFX();
        
        if (currentHP <= 0f) 
        {
            Die();
        }
    }

    /// 체력 회복 아이템 처리 함수, amount는 회복량
    public void HealDamage(float amount){
        // 이미 현재 체력이 최대 체력 이상이면 힐 안하고 리턴.
        if(currentHP >= maxHP){
            return;
        }
        
        // 현재 체력에서 amount 만큼 증가.
        currentHP += amount;
        
        // 힐 사운드 재생.
        audioSource.PlayOneShot(healSound);

        Debug.Log(amount + "만큼 타겟 엔티티 회복");
        hpBar.SetHP(maxHP, currentHP);

    }

    private void TriggerDamageFX()
    {
        if (damageVolume == null || damageProfile == null) return;
        if (damageFxCoroutine != null) StopCoroutine(damageFxCoroutine);
        damageFxCoroutine = StartCoroutine(DamageFlashFX());
    }

    private System.Collections.IEnumerator DamageFlashFX()
    {
        VolumeProfile prevProfile = damageVolume.profile;
        if (normalProfile == null) normalProfile = prevProfile;

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

    public bool isFull(){
        return (currentHP == maxHP);
    }

    

    /// hp가 0이 될 때, Die.
    private void Die()
{
    Debug.Log("죽음에 이르렀도다...");

    if (isPlayer)
    {
        Debug.Log("플레이어 사망, DeathManager 호출 시도");
        deathManager.ShowDeathUI();
            Debug.Log("DM의 메소드 실행 요청");
        }
    
    else{
        float destroyDelay = 0f;
        Destroy(gameObject, destroyDelay);

    }
}   
}

