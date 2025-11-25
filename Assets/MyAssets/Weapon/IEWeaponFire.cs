using UnityEngine;

public interface IEWeaponFire
{
    string WeaponId { get; }
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    float DamagePerShot { get; }
    bool CanFire { get; }

    void Fire();
    void Reload();
    void AddAmmoBonus(int amount, bool refill);
    void AddDamageBonus(float amount);
}
