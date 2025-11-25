using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI References")]
    public Image foregroundImage; // 对应 Foreground Image

    public Color fullHealthColor = Color.green;
    public Color zeroHealthColor = Color.red;

    private void Awake()
    {
        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            
        }
    }

    private void UpdateHealthUI()
    {
        if (foregroundImage != null)
        {
            // 假设 Foreground 是填充 Image，使用 fillAmount
            foregroundImage.fillAmount = (float)currentHealth / maxHealth;
            foregroundImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, (float)currentHealth / maxHealth);
        }
    }
}
