using UnityEngine;

/// <summary>
/// Stores ammo per magazine so each mag keeps its own remaining rounds.
/// </summary>
public class MagazineAmmo : MonoBehaviour
{
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int startingAmmo = -1; // -1 means use maxAmmo

    public int MaxAmmo => maxAmmo;
    public int CurrentAmmo { get; private set; }
    public bool HasAmmo => CurrentAmmo > 0;

    void Awake()
    {
        if (startingAmmo < 0)
            startingAmmo = maxAmmo;
        CurrentAmmo = Mathf.Clamp(startingAmmo, 0, maxAmmo);
    }

    public void Refill()
    {
        CurrentAmmo = maxAmmo;
    }

    public void SetAmmo(int amount)
    {
        CurrentAmmo = Mathf.Clamp(amount, 0, maxAmmo);
    }

    public int Consume(int amount = 1)
    {
        if (CurrentAmmo <= 0 || amount <= 0)
            return 0;

        int used = Mathf.Min(amount, CurrentAmmo);
        CurrentAmmo -= used;
        return used;
    }
}
