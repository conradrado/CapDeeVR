using UnityEngine;

public class TargetEntity : MonoBehaviour
{
    [Header("스탯")]
    // 해당 엔티티, NPC의 최대 체력
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private Animator dmgAnimator;
    [SerializeField] private HPBar hpBar;

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

    /// hp가 0이 될 때, Die.
     private void Die()
    {
        Debug.Log("죽음에 이르렀도다...");

        if (dmgAnimator != null)
        {
            dmgAnimator.SetTrigger("Dead");
        }

        // 일정 시간 뒤에 파괴
        float destroyDelay = 3f; // 애니메이션 길이에 맞춰 조절절
        Destroy(gameObject, destroyDelay);
    }
}
