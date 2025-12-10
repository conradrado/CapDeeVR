using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSlideMover : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    private Vector3 lastPosition;
    private Vector3 accumulatedMovement;
    public float moveThreshold = 0.1f; // 이동 판정 임계값
    public string allowedDirection = "Horizontal"; // 또는 "Vertical"

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        lastPosition = interactor.transform.position;
        accumulatedMovement = Vector3.zero;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
        accumulatedMovement = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (interactor == null) return;

        Vector3 currentPosition = interactor.transform.position;
        Vector3 delta = currentPosition - lastPosition;
        accumulatedMovement += delta;

        if (allowedDirection == "Horizontal")
        {
            if (Mathf.Abs(accumulatedMovement.x) >= moveThreshold)
            {
                float dir = Mathf.Sign(accumulatedMovement.x);
                Vector3 movement = new Vector3(dir, 0, 0);
                rb.MovePosition(rb.position + movement);
                accumulatedMovement = Vector3.zero;
            }
        }
        else if (allowedDirection == "Vertical")
        {
            if (Mathf.Abs(accumulatedMovement.z) >= moveThreshold)
            {
                float dir = Mathf.Sign(accumulatedMovement.z);
                Vector3 movement = new Vector3(0, 0, dir);
                rb.MovePosition(rb.position + movement);
                accumulatedMovement = Vector3.zero;
            }
        }

        lastPosition = currentPosition;
    }
}

