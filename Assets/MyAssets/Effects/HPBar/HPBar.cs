using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetMaxHP(float maxHP)
    {
        // fillAmount is normalized 0~1, so max is always 1
        _healthbarSprite.fillAmount = 1f;
    }

    public void SetHP(float maxHP, float curHP){
        _healthbarSprite.fillAmount = curHP / maxHP;
    }

}
