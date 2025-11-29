using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] int startingGold = 0;

    public UnityEvent<int> onGoldChanged = new UnityEvent<int>();

    public int CurrentGold { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentGold = startingGold;
        onGoldChanged.Invoke(CurrentGold);
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;
        CurrentGold += amount;
        onGoldChanged.Invoke(CurrentGold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return true;
        if (CurrentGold < amount)
            return false;

        CurrentGold -= amount;
        onGoldChanged.Invoke(CurrentGold);
        return true;
    }
}
