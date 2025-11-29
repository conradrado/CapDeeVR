using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string Name;
        public GameObject Prefab;
        public int Cost = 10;
        public Transform SpawnPoint;
    }

    [SerializeField] ShopItem[] items;
    [SerializeField] GameObject shopPanel;
    [SerializeField] TMP_Text goldText;

    void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.onGoldChanged.AddListener(OnGoldChanged);
        UpdateGoldUI();
    }

    void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.onGoldChanged.RemoveListener(OnGoldChanged);
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
        UpdateGoldUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OnWaveCompleted(int waveIndex)
    {
        OpenShop();
    }

    public void BuyItem(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
            return;

        var item = items[index];
        if (item.Prefab == null || item.SpawnPoint == null)
            return;

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[ShopManager] CurrencyManager missing.");
            return;
        }

        if (!CurrencyManager.Instance.SpendGold(item.Cost))
        {
            Debug.Log("Not enough gold.");
            return;
        }

        Instantiate(item.Prefab, item.SpawnPoint.position, item.SpawnPoint.rotation);
        UpdateGoldUI();
    }

    void OnGoldChanged(int amount)
    {
        UpdateGoldUI();
    }

    void UpdateGoldUI()
    {
        if (goldText != null && CurrencyManager.Instance != null)
            goldText.text = $"Gold: {CurrencyManager.Instance.CurrentGold}";
    }
}
