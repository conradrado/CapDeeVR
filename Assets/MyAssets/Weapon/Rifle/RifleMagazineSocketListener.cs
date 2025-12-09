using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RifleMagazineSocketListener : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private GunFire gunFire;
    [SerializeField] private Transform magazineAttachPoint;
    [SerializeField] private Vector3 ejectForce = new Vector3(0, 0.5f, -1f);
    [SerializeField] private Vector3 ejectTorque = new Vector3(0.2f, 0.4f, 0.2f);
    [SerializeField] private bool lockToAttachPoint = true;

    private XRGrabInteractable currentMagazine;
    private bool hasDroppedFromEmpty;
    private bool previousHoverMeshState = true;
    private InteractableSelectMode previousSelectMode = InteractableSelectMode.Single;
    private bool previousSocketActive = true;

    private void Awake()
    {
        if (socket != null && magazineAttachPoint != null)
            socket.attachTransform = magazineAttachPoint;
    }

    private void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(OnMagazineInserted);
        }

        if (gunFire != null)
            gunFire.OnAmmoDepleted += HandleAmmoEmpty;
    }

    private void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnMagazineInserted);
        }

        if (gunFire != null)
            gunFire.OnAmmoDepleted -= HandleAmmoEmpty;

        UnsubscribeMagazineEvents();
    }

    private void Update()
    {
        if (currentMagazine == null || gunFire == null || hasDroppedFromEmpty)
            return;

        // Keep the mag hard-locked to the attach point so child meshes do not drift.
        MaintainLockedPose();

        // Drop the mag as soon as ammo hits zero (without waiting for an empty trigger pull).
        if (gunFire.CurrentAmmo <= 0)
            DropCurrentMagazine(applyImpulse: true);
    }

    private void OnMagazineInserted(SelectEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
            return;

        // Make sure the player's interactor lets go once the mag is in the socket.
        if (args.interactorObject is XRBaseInteractor interactor && interactor.hasSelection)
        {
            var manager = interactor.interactionManager;
            if (manager != null && args.interactableObject is IXRSelectInteractable interactableToRelease)
                manager.SelectExit(interactor as IXRSelectInteractor, interactableToRelease);
        }

        // Free any previous mag subscriptions before wiring the new one.
        UnsubscribeMagazineEvents();
        currentMagazine = args.interactableObject.transform.GetComponent<XRGrabInteractable>();
        hasDroppedFromEmpty = false;

        if (currentMagazine == null)
            return;

        previousSelectMode = currentMagazine.selectMode;
        currentMagazine.selectMode = InteractableSelectMode.Multiple; // allow socket + hand to co-select so 손으로 뺄 수 있음

        SnapMagazineToAttachPoint(args.interactableObject.transform);
        ConfigureMagazinePhysics(isAttached: true);
        DisableSocketTracking();

        currentMagazine.selectEntered.AddListener(OnMagazineGrabbed);
        gunFire?.Reload();

        // Hide socket visuals while a mag is seated.
        if (socket != null)
        {
            previousHoverMeshState = socket.showInteractableHoverMeshes;
            socket.showInteractableHoverMeshes = false;
        }
    }

    private void OnMagazineGrabbed(SelectEnterEventArgs args)
    {
        // Ignore socket re-selects; drop only when a player grabs it.
        if (args != null && args.interactorObject == socket)
            return;

        DropCurrentMagazine(applyImpulse: false);
    }

    private void HandleAmmoEmpty()
    {
        DropCurrentMagazine(applyImpulse: true);
    }

    private void DropCurrentMagazine(bool applyImpulse = false)
    {
        if (currentMagazine == null)
            return;

        hasDroppedFromEmpty = true;
        currentMagazine.selectEntered.RemoveListener(OnMagazineGrabbed);

        // Ensure the socket no longer holds the mag.
        if (socket != null && socket.interactionManager != null && currentMagazine is IXRSelectInteractable interactable)
            socket.interactionManager.SelectExit(socket as IXRSelectInteractor, interactable);

        DetachMagazineTransform();
        ConfigureMagazinePhysics(isAttached: false);
        RestoreSocketTracking();

        if (currentMagazine != null)
        {
            currentMagazine.selectMode = previousSelectMode;

            if (applyImpulse && currentMagazine.TryGetComponent(out Rigidbody rb))
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

        currentMagazine = null;

        // Allow socket to visualize the next magazine.
        if (socket != null)
            socket.showInteractableHoverMeshes = previousHoverMeshState;
    }

    private void MaintainLockedPose()
    {
        if (!lockToAttachPoint || magazineAttachPoint == null || currentMagazine == null)
            return;

        var t = currentMagazine.transform;

        if (t.parent != magazineAttachPoint)
            t.SetParent(magazineAttachPoint, worldPositionStays: false);

        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    private void SnapMagazineToAttachPoint(Transform magazineTransform)
    {
        if (magazineTransform == null || magazineAttachPoint == null)
            return;

        magazineTransform.SetParent(magazineAttachPoint, worldPositionStays: false);
        magazineTransform.localPosition = Vector3.zero;
        magazineTransform.localRotation = Quaternion.identity;
        magazineTransform.localScale = Vector3.one;
    }

    private void DetachMagazineTransform()
    {
        if (currentMagazine == null)
            return;

        Transform magazineTransform = currentMagazine.transform;
        magazineTransform.SetParent(null, worldPositionStays: true);
    }

    private void ConfigureMagazinePhysics(bool isAttached)
    {
        if (currentMagazine == null)
            return;

        if (currentMagazine.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = isAttached;
            rb.useGravity = !isAttached;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = isAttached ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        }

        if (currentMagazine.TryGetComponent(out Collider col))
            col.isTrigger = isAttached;
    }

    private void UnsubscribeMagazineEvents()
    {
        if (currentMagazine != null)
            currentMagazine.selectEntered.RemoveListener(OnMagazineGrabbed);
    }

    private void DisableSocketTracking()
    {
        if (!lockToAttachPoint || socket == null)
            return;

        previousSocketActive = socket.socketActive;
        socket.socketActive = false;
    }

    private void RestoreSocketTracking()
    {
        if (!lockToAttachPoint || socket == null)
            return;

        socket.socketActive = previousSocketActive;
    }
}
