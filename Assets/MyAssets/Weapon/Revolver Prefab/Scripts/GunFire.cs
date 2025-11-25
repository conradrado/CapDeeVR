using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GunFire : MonoBehaviour, IEWeaponFire
{
    [Header("Bullet / Muzzle")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Weapon Stats")]
    [SerializeField] private float bulletForce = 20f;
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private float extraDamage = 0f; // persistent bonus from upgrades
    public event System.Action OnAmmoDepleted;


    [Header("HUD")]
    [SerializeField] private TextMeshPro ammoHud;

    [Header("Animator")]
    [SerializeField] private Animator gunAnimator;

    [Header("Sound Effect")]
    [SerializeField] private AudioSource fireSFX;
    [SerializeField] private AudioClip gunShotClip;
    [SerializeField] private AudioClip gunSpin;

    [Header("Muzzle FX")]
    [SerializeField] private Light muzzleFlame;
    [SerializeField] private ParticleSystem muzzleParticle;

    private int currentAmmo;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    public string WeaponId => name;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo + PlayerStat.AmmoBonus;
    public float DamagePerShot => GetBaseDamage() + extraDamage;
    public bool CanFire => currentAmmo > 0;

    private IEnumerator FlashMuzzle()
    {
        if (muzzleFlame == null)
            yield break;

        muzzleFlame.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        muzzleFlame.gameObject.SetActive(false);
    }

    /* ---------- Life-cycle ---------- */
    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        currentAmmo = MaxAmmo;

        if (muzzleFlame != null)
            muzzleFlame.gameObject.SetActive(false);

        grab.activated.AddListener(OnActivated);

        grab.selectEntered.AddListener(_ => UpdateHud());
        grab.selectExited.AddListener(_ => UpdateHud());
    }

    private void OnDestroy()
    {
        grab.activated.RemoveListener(OnActivated);
    }

    /* ---------- Events ---------- */
    private void OnActivated(ActivateEventArgs args)
    {
        Fire();

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor &&
            controllerInteractor.xrController != null)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.7f, 0.15f);
        }
    }

    /* ---------- Fire Logic ---------- */
    public void Fire()
    {
        if (currentAmmo <= 0)
        {
            OnAmmoDepleted?.Invoke();
            if (ammoHud != null)
                ammoHud.text = "RELOAD!";

            if (gunAnimator != null)
                gunAnimator.SetTrigger("Empty");
            return;
        }

        if (!bulletPrefab || !firePoint)
            return;

        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

        if (extraDamage != 0f && bullet.TryGetComponent(out BulletDamage bulletDamage))
            bulletDamage.AddDamage(extraDamage);

        if (fireSFX != null && gunShotClip != null)
            fireSFX.PlayOneShot(gunShotClip);

        StartCoroutine(FlashMuzzle());

        if (gunAnimator != null)
            gunAnimator.SetTrigger("FireGun");

        currentAmmo--;
        UpdateHud();
    }

    /* ---------- Utils ---------- */
    public void Reload()
    {
        currentAmmo = MaxAmmo;
        UpdateHud();
    }

    public void IncreaseMaxAmmo(int amount, bool refill)
    {
        AddAmmoBonus(amount, refill);
    }

    public void AddAmmoBonus(int amount, bool refill)
    {
        if (amount <= 0)
            return;

        maxAmmo += amount;
        if (refill)
            currentAmmo = MaxAmmo;
        else
            currentAmmo = Mathf.Min(currentAmmo, MaxAmmo);

        UpdateHud();
    }

    public void AddDamageBonus(float amount)
    {
        extraDamage += amount;
    }

    float GetBaseDamage()
    {
        if (bulletPrefab != null && bulletPrefab.TryGetComponent(out BulletDamage bulletDamage))
            return bulletDamage.Damage;
        return 0f;
    }

    private void UpdateHud()
    {
        if (currentAmmo > MaxAmmo)
            currentAmmo = MaxAmmo;
        if (ammoHud != null)
            ammoHud.text = $"{currentAmmo} / {MaxAmmo}";
    }
}
