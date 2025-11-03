using UnityEngine;

public class TargetEntity : MonoBehaviour
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

    // 해당 엔티티, NPC의 현재 체력
    private float currentHP;
    private bool isDead = false;
    // Awake 함수는 Start 함수보다 먼저 실행됨
    private void Awake()
    {
        // 현재 체력 = 최대 체력
        currentHP = maxHP;
        hpBar.SetMaxHP(maxHP);
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

    public bool isFull(){
        return (currentHP == maxHP);
    }

    

    /// hp가 0이 될 때, Die.
     private void Die()
    {
        Debug.Log("죽음에 이르렀도다...");

        // 플레이어의 경우 DeathManager의 게임오버 함수 호출
        if (isPlayer){ 
            Debug.Log("dm호출");
            DeathManager.Instance.ShowDeathUI();
        }
        else{
            // 일정 시간 뒤에 파괴
            float destroyDelay = 0f; // 애니메이션 길이에 맞춰 조절절
            Destroy(gameObject, destroyDelay);
        }

    }
}
