using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RifleShootController : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float shootForce = 25f;
    [SerializeField] private float fireCooldown = 0.1f;
    [SerializeField] private bool autoFireOnHold = true;

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private bool autoDropOnEmpty = true;
    [SerializeField] private bool requireMagazine = true;

    [Header("Magazine")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor magazineSocket;
    [SerializeField] private Transform magazineAttachPoint;
    [SerializeField] private Vector3 ejectForce = new Vector3(0, 0.75f, -1.5f);
    [SerializeField] private Vector3 ejectTorque = new Vector3(0.2f, 0.4f, 0.2f);
    [SerializeField] private bool lockMagazineToAttachPoint = true;
    [SerializeField] private bool destroyMagazineOnRemove = true;
    [SerializeField] private bool useTriggerWhenAttached = true;
    [SerializeField] private bool ignoreGunCollisionsOnAttach = true;
    [SerializeField] private Collider[] gunColliders;

    [Header("FX")]
    [SerializeField] private AudioSource fireSFX;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private ParticleSystem muzzleFX;
    [SerializeField] private GameObject muzzleFlashObject; // assign a child at the muzzle, keep disabled
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    [SerializeField] private Animator animator; 

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private float lastFireTime;
    private Coroutine fireRoutine;
    private bool isFiring;
    private Coroutine muzzleFlashRoutine;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentMagazine;
    private Transform currentMagazineTransform;
    private MagazineAmmo currentMagazineAmmo;
    private bool hasDroppedFromEmpty;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode previousSelectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single;
    private bool selectModeCached;
    private readonly List<(Collider mag, Collider gun)> ignoredCollisionPairs = new();

    [Header("Cleanup")]
    [SerializeField] private bool destroyDroppedMagazines = true;
    [SerializeField] private float destroyDelaySeconds = 8f;

    void Update()
    {
        // Failsafe: if ammo hit zero but drop didn't trigger (timing/edge cases), enforce here.
        if (autoDropOnEmpty && currentMagazineAmmo != null && !currentMagazineAmmo.HasAmmo && !hasDroppedFromEmpty)
            HandleEmptyMagazine();
    }

    

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (muzzleFlashObject != null)
            muzzleFlashObject.SetActive(false);
    }

    void OnEnable()
    {
        if (grab != null)
        {
            grab.activated.AddListener(OnActivated);
            grab.deactivated.AddListener(OnDeactivated);
        }
        if (magazineSocket != null)
        {
            magazineSocket.selectEntered.AddListener(OnMagazineInserted);
            magazineSocket.selectExited.AddListener(OnMagazineRemoved);
        }

        // If a magazine is already seated in the attach point at scene start, cache it so it can be grabbed and removed.
        if (currentMagazineTransform == null && magazineAttachPoint != null && magazineAttachPoint.childCount > 0)
        {
            var seated = magazineAttachPoint.GetChild(0);
            if (seated != null && seated.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>())
            {
                currentMagazineTransform = seated;
                currentMagazine = seated.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                currentMagazineAmmo = seated.GetComponent<MagazineAmmo>() ?? seated.GetComponentInParent<MagazineAmmo>();
                CacheAndAllowMultiSelect();
                currentMagazine.selectEntered.AddListener(OnMagazineGrabbed);
            }
        }
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.activated.RemoveListener(OnActivated);
            grab.deactivated.RemoveListener(OnDeactivated);
        }
        if (magazineSocket != null)
        {
            magazineSocket.selectEntered.RemoveListener(OnMagazineInserted);
            magazineSocket.selectExited.RemoveListener(OnMagazineRemoved);
        }
        StopFiringLoop();
        HideMuzzleFlash();
    }

    void OnActivated(ActivateEventArgs args)
    {
        if (autoFireOnHold)
        {
            StartFiringLoop();
        }
        else
        {
            Fire();
        }

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor &&
            controllerInteractor.xrController != null)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.7f, 0.1f);
        }
    }

    void OnDeactivated(DeactivateEventArgs args)
    {
        StopFiringLoop();
    }

    void OnMagazineInserted(SelectEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
            return;

        currentMagazineTransform = args.interactableObject.transform;
        currentMagazine = currentMagazineTransform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (currentMagazine == null)
            currentMagazine = currentMagazineTransform.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        currentMagazineAmmo = currentMagazineTransform.GetComponent<MagazineAmmo>() ??
                              currentMagazineTransform.GetComponentInParent<MagazineAmmo>();

        CacheAndAllowMultiSelect();
        currentMagazine.selectEntered.AddListener(OnMagazineGrabbed);

        hasDroppedFromEmpty = false;

        AttachMagazineTransform(currentMagazineTransform);
        ConfigureMagazinePhysics(isAttached: true);
    }

    void OnMagazineRemoved(SelectExitEventArgs args)
    {
        if (args == null || args.interactableObject == null)
            return;

        if (currentMagazineTransform == null || args.interactableObject.transform != currentMagazineTransform)
            return;

        RemoveCurrentMagazine();
    }

    void OnMagazineGrabbed(SelectEnterEventArgs args)
    {
        // If a hand grabs it while socket holds it, release from socket and treat as removed.
        if (args != null && magazineSocket != null && args.interactorObject == magazineSocket)
            return;

        RemoveCurrentMagazine();
    }

    void StartFiringLoop()
    {
        if (isFiring)
            return;

        isFiring = true;
        fireRoutine = StartCoroutine(FireContinuously());
    }

    void StopFiringLoop()
    {
        if (!isFiring)
            return;

        isFiring = false;
        if (fireRoutine != null)
            StopCoroutine(fireRoutine);
        fireRoutine = null;
    }

    System.Collections.IEnumerator FireContinuously()
    {
        while (isFiring)
        {
            Fire();
            yield return new WaitForSeconds(fireCooldown);
        }
    }

    public void Fire()
    {
        if (Time.time < lastFireTime + fireCooldown)
            return;

        if (requireMagazine && currentMagazineAmmo == null)
            return;

        if (currentMagazineAmmo != null && !currentMagazineAmmo.HasAmmo)
        {
            HandleEmptyMagazine();
            return;
        }

        if (bulletPrefab == null || muzzle == null)
            return;

        lastFireTime = Time.time;

        var bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.AddForce(muzzle.forward * shootForce, ForceMode.Impulse);

        currentMagazineAmmo?.Consume(1);

        if (fireSFX != null && fireClip != null)
            fireSFX.PlayOneShot(fireClip);

        if (muzzleFX != null)
            muzzleFX.Play();

        if (animator != null)
            animator.Play("Shot");

        if (muzzleFlashObject != null)
        {
            if (muzzleFlashRoutine != null)
                StopCoroutine(muzzleFlashRoutine);
            muzzleFlashRoutine = StartCoroutine(FlashMuzzleOnce());
        }

        if (currentMagazineAmmo != null && !currentMagazineAmmo.HasAmmo)
            HandleEmptyMagazine();
    }

    System.Collections.IEnumerator FlashMuzzleOnce()
    {
        muzzleFlashObject.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashDuration);
        HideMuzzleFlash();
    }

    void HideMuzzleFlash()
    {
        if (muzzleFlashObject != null)
            muzzleFlashObject.SetActive(false);
    }

    void HandleEmptyMagazine()
    {
        if (hasDroppedFromEmpty || !autoDropOnEmpty)
            return;

        hasDroppedFromEmpty = true;
        StopFiringLoop();
        DropMagazine();
    }

    void Reload()
    {
        if (currentMagazineAmmo != null)
            currentMagazineAmmo.Refill();
    }

    void DropMagazine()
    {
        if (currentMagazine == null && currentMagazineTransform == null)
            return;

        DetachMagazineTransform();
        ConfigureMagazinePhysics(isAttached: false);

        if (destroyDroppedMagazines)
        {
            var go = currentMagazine != null ? currentMagazine.gameObject : currentMagazineTransform?.gameObject;
            if (go != null)
                Destroy(go, destroyDelaySeconds);
        }

        currentMagazine = null;
        currentMagazineTransform = null;
        currentMagazineAmmo = null;
        RestoreSelectMode();
        ignoredCollisionPairs.Clear();
    }

    void RemoveCurrentMagazine()
    {
        if (currentMagazineTransform == null)
            return;

        DetachMagazineTransform();
        ConfigureMagazinePhysics(isAttached: false);

        if (destroyMagazineOnRemove)
            Destroy(currentMagazineTransform.gameObject);

        currentMagazine = null;
        currentMagazineTransform = null;
        currentMagazineAmmo = null;
        hasDroppedFromEmpty = false;
        RestoreSelectMode();
        ignoredCollisionPairs.Clear();
    }

    void AttachMagazineTransform(Transform magazineTransform)
    {
        if (magazineTransform == null || magazineAttachPoint == null || !lockMagazineToAttachPoint)
            return;

        magazineTransform.SetParent(magazineAttachPoint, worldPositionStays: false);
        magazineTransform.localPosition = Vector3.zero;
        magazineTransform.localRotation = Quaternion.identity;
        magazineTransform.localScale = Vector3.one;
    }

    void DetachMagazineTransform()
    {
        if (currentMagazineTransform == null)
            return;

        currentMagazineTransform.SetParent(null, worldPositionStays: true);
    }

    void ConfigureMagazinePhysics(bool isAttached)
    {
        var targetTransform = currentMagazineTransform ?? currentMagazine?.transform;
        if (targetTransform == null)
            return;

        if (targetTransform.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = isAttached;
            rb.useGravity = !isAttached;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = isAttached ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;

            if (!isAttached)
            {
                Vector3 forceDir = magazineAttachPoint != null
                    ? magazineAttachPoint.TransformDirection(ejectForce)
                    : transform.TransformDirection(ejectForce);
                Vector3 torqueDir = magazineAttachPoint != null
                    ? magazineAttachPoint.TransformDirection(ejectTorque)
                    : transform.TransformDirection(ejectTorque);

                rb.AddForce(forceDir, ForceMode.Impulse);
                rb.AddTorque(torqueDir, ForceMode.Impulse);
            }
        }

        var magColliders = GetMagazineColliders(targetTransform);

        if (useTriggerWhenAttached)
        {
            foreach (var col in magColliders)
                col.isTrigger = isAttached;
        }
        else if (ignoreGunCollisionsOnAttach)
        {
            ApplyCollisionIgnores(isAttached, magColliders);
        }
    }

    void CacheAndAllowMultiSelect()
    {
        if (currentMagazine == null)
            return;

        previousSelectMode = currentMagazine.selectMode;
        selectModeCached = true;
        currentMagazine.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;
    }

    void RestoreSelectMode()
    {
        if (currentMagazine == null || !selectModeCached)
            return;

        currentMagazine.selectMode = previousSelectMode;
        selectModeCached = false;
        currentMagazine.selectEntered.RemoveListener(OnMagazineGrabbed);
    }

    List<Collider> GetMagazineColliders(Transform target)
    {
        var list = new List<Collider>();
        if (target == null)
            return list;

        target.GetComponents(list);
        target.GetComponentsInChildren(true, list);
        return list;
    }

    void ApplyCollisionIgnores(bool enable, List<Collider> magColliders)
    {
        if (!ignoreGunCollisionsOnAttach || gunColliders == null || gunColliders.Length == 0 || magColliders == null)
            return;

        if (enable)
        {
            ignoredCollisionPairs.Clear();
            foreach (var mag in magColliders)
            {
                if (mag == null) continue;
                foreach (var gun in gunColliders)
                {
                    if (gun == null) continue;
                    Physics.IgnoreCollision(mag, gun, true);
                    ignoredCollisionPairs.Add((mag, gun));
                }
            }
        }
        else
        {
            foreach (var pair in ignoredCollisionPairs)
            {
                if (pair.mag != null && pair.gun != null)
                    Physics.IgnoreCollision(pair.mag, pair.gun, false);
            }
            ignoredCollisionPairs.Clear();
        }
    }
}
