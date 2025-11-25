using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class NovaGun : MonoBehaviour, IEWeaponFire
{
    [Header("Projectile")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float shootForce = 25f;

    [Header("Weapon Stats")]
    [SerializeField] private int maxAmmo = 8;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private float baseDamage = 30f;
    [SerializeField] private float extraDamage = 0f;

    [Header("Shotgun Spread")]
    [SerializeField] private int pellets = 8;
    [SerializeField] private float spread = 5f;

    [Header("FX")]
    [SerializeField] private AudioSource fireSFX;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private ParticleSystem muzzleFX;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private TextMeshPro ammoHud;
    [Header("Pump")]
    [SerializeField] private PumpSlide pumpSlide;
    [SerializeField] private AudioSource pumpAudio;
    [SerializeField] private AudioClip pumpBackClip;
    [SerializeField] private AudioClip pumpForwardClip;
    [SerializeField] private AudioClip emptyClickClip;
    [Header("Shell Eject")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform ejectPoint;
    [SerializeField] private float ejectForce = 1.5f;
    [SerializeField] private Vector3 ejectTorque = new Vector3(0.2f, 0.6f, 0.2f);

    private int currentAmmo;
    private float lastFireTime;
    private bool isChambered = true;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private UnityAction<SelectEnterEventArgs> selectEnterHandler;
    private UnityAction<SelectExitEventArgs> selectExitHandler;

    public string WeaponId => name;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo + PlayerStat.AmmoBonus;
    public float DamagePerShot => baseDamage + extraDamage;
    public bool CanFire => currentAmmo > 0 && isChambered && Time.time >= lastFireTime + fireCooldown;
    public bool IsChambered => isChambered;

    void Awake()
    {
        currentAmmo = MaxAmmo;
        CacheDamageFromPrefab();
        isChambered = currentAmmo > 0;
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (pumpSlide == null)
            pumpSlide = GetComponentInChildren<PumpSlide>();

        if (pumpSlide != null)
        {
            pumpSlide.OnPumpBack += HandlePumpBack;
            pumpSlide.OnPumpForward += HandlePumpForward;
        }

        if (grab != null)
        {
            grab.activated.AddListener(OnActivated);
            if (selectEnterHandler == null) selectEnterHandler = OnSelectEnter;
            if (selectExitHandler == null) selectExitHandler = OnSelectExit;
            grab.selectEntered.AddListener(selectEnterHandler);
            grab.selectExited.AddListener(selectExitHandler);
        }

        UpdateHud();
    }

    void OnDisable()
    {
        if (pumpSlide != null)
        {
            pumpSlide.OnPumpBack -= HandlePumpBack;
            pumpSlide.OnPumpForward -= HandlePumpForward;
        }

        if (grab != null)
        {
            grab.activated.RemoveListener(OnActivated);
            if (selectEnterHandler != null) grab.selectEntered.RemoveListener(selectEnterHandler);
            if (selectExitHandler != null) grab.selectExited.RemoveListener(selectExitHandler);
        }
    }

    void CacheDamageFromPrefab()
    {
        if (bulletPrefab != null && bulletPrefab.TryGetComponent(out BulletDamage bulletDamage))
            baseDamage = bulletDamage.Damage;
    }

    public void Fire()
    {
        if (!CanFire)
        {
            if ((currentAmmo <= 0 || !isChambered) && emptyClickClip != null && pumpAudio != null)
                pumpAudio.PlayOneShot(emptyClickClip);
            return;
        }

        if (bulletPrefab == null || muzzle == null)
            return;

        lastFireTime = Time.time;

        for (int i = 0; i < Mathf.Max(1, pellets); i++)
        {
            Vector3 randomSpread = muzzle.forward + Random.insideUnitSphere * spread;
            randomSpread.Normalize();

            var bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(randomSpread));
            if (bullet.TryGetComponent(out Rigidbody rb))
                rb.AddForce(randomSpread * shootForce, ForceMode.Impulse);

            if (bullet.TryGetComponent(out BulletDamage bulletDamage))
                bulletDamage.SetDamage(DamagePerShot);
        }

        if (fireSFX != null && fireClip != null)
            fireSFX.PlayOneShot(fireClip);

        if (muzzleFX != null)
            muzzleFX.Play();
        if (muzzleFlashPrefab != null && muzzle != null)
            Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation);

        
        isChambered = false; // 펌프 후 진입 전까지 발사 불가
        UpdateHud();
    }

    public void Reload()
    {
        currentAmmo = MaxAmmo;
        isChambered = currentAmmo > 0;
        UpdateHud();
    }

    public void AddAmmoBonus(int amount, bool refill)
    {
        if (amount <= 0)
            return;

        maxAmmo += amount;
        currentAmmo = refill ? MaxAmmo : Mathf.Min(currentAmmo, MaxAmmo);
        if (refill)
            isChambered = currentAmmo > 0;
        UpdateHud();
    }

    public void AddDamageBonus(float amount)
    {
        extraDamage += amount;
    }

    void UpdateHud()
    {
        if (currentAmmo > MaxAmmo)
            currentAmmo = MaxAmmo;
        if (ammoHud != null)
            ammoHud.text = $"{currentAmmo} / {MaxAmmo}";
    }

    void OnActivated(ActivateEventArgs args)
    {
        Fire();

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor &&
            controllerInteractor.xrController != null)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.7f, 0.15f);
        }
    }

    void HandlePumpBack()
    {
        if (currentAmmo > 0)
        {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);

            if (shellPrefab != null && ejectPoint != null)
            {
                var shell = Instantiate(shellPrefab, ejectPoint.position, ejectPoint.rotation);
                if (shell.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 fwd = (ejectPoint.forward * ejectForce) + (Random.insideUnitSphere * 0.2f);
                    rb.AddForce(fwd, ForceMode.Impulse);
                    rb.AddTorque(new Vector3(
                        Random.Range(-ejectTorque.x, ejectTorque.x),
                        Random.Range(-ejectTorque.y, ejectTorque.y),
                        Random.Range(-ejectTorque.z, ejectTorque.z)
                    ), ForceMode.Impulse);
                }
            }
        }

        if (pumpAudio != null && pumpBackClip != null)
            pumpAudio.PlayOneShot(pumpBackClip);
        isChambered = false;
        UpdateHud();
    }

    void HandlePumpForward()
    {
        if (pumpAudio != null && pumpForwardClip != null)
            pumpAudio.PlayOneShot(pumpForwardClip);
        if (currentAmmo > 0)
            isChambered = true;
        UpdateHud();
    }

    void OnSelectEnter(SelectEnterEventArgs args) => UpdateHud();
    void OnSelectExit(SelectExitEventArgs args) => UpdateHud();
}
