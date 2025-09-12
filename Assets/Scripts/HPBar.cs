using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetMaxHP(float maxHP)
    {
        _healthbarSprite.fillAmount = maxHP;
    }

    public void SetHP(float maxHP, float curHP){
        _healthbarSprite.fillAmount = curHP / maxHP;
    }

}
