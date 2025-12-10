using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PumpSlide : MonoBehaviour
{
    [Header("슬라이드 범위 (총의 로컬 기준)")]
    public Transform slideStart;   // 슬라이드 전진 (0) 기준
    public Transform slideEnd;     // 슬라이드 후퇴 (1) 기준

    [Header("총기 본체 Transform (Shotgun)")]
    public Transform referenceRoot; // Shotgun Transform
    public float returnSpeed = 5f;
    [Range(0f, 1f)] public float backThreshold = 0.9f;
    [Range(0f, 1f)] public float forwardThreshold = 0.1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    private float grabOffsetZ;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private bool reachedBack;
    private bool reachedForward;
    private bool cycledOnce;

    // 스케일이 (1,1,1)에서 틀어지는 것을 막기 위한 고정값
    private static readonly Vector3 FixedScale = Vector3.one;

    public float SlideProgress { get; private set; } // 0 = forward, 1 = fully back
    public event System.Action OnPumpBack;
    public event System.Action OnPumpForward;
    public event System.Action OnPumpCycleComplete; // back -> forward

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void Start()
    {
        if (referenceRoot == null)
        {
            referenceRoot = transform.parent;
            Debug.LogWarning($"[PumpSlide] referenceRoot가 비어있어 부모({referenceRoot.name})로 자동 지정");
        }

        // 최초에도 스케일을 (1,1,1)로 고정
        transform.localScale = FixedScale;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

        // 부모에 유지되며, 스케일은 강제로 (1,1,1)
        transform.SetParent(referenceRoot, true);
        transform.localScale = FixedScale;

        // 손의 현재 위치를 기준으로 로컬 좌표 오프셋 계산
        Vector3 handLocal = referenceRoot.InverseTransformPoint(interactor.transform.position);
        grabOffsetZ = handLocal.z - transform.localPosition.z;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;

        // 부모/스케일 복원
        transform.SetParent(referenceRoot, true);
        transform.localScale = FixedScale;

        StopAllCoroutines();
        StartCoroutine(ReturnToOrigin());
    }

    void LateUpdate()
    {
        // 외부 변환에 의해 스케일이 틀어지지 않도록 매 프레임 보정
        transform.localScale = FixedScale;

        if (interactor == null) return;

        Vector3 handLocal = referenceRoot.InverseTransformPoint(interactor.transform.position);
        float startZ = slideStart.localPosition.z;
        float endZ = slideEnd.localPosition.z;
        float zMin = Mathf.Min(startZ, endZ);
        float zMax = Mathf.Max(startZ, endZ);

        float newZ = Mathf.Clamp(handLocal.z - grabOffsetZ, zMin, zMax);
        transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, newZ);
        transform.localRotation = initialLocalRot;

        // Use the actual start/end ordering so progress 0 = slideStart, 1 = slideEnd regardless of sign
        SlideProgress = Mathf.InverseLerp(startZ, endZ, newZ);
        EvaluatePumpEvents();
    }

    private System.Collections.IEnumerator ReturnToOrigin()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.localPosition = Vector3.Lerp(startPos, initialLocalPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, initialLocalRot, t);
            transform.localScale = FixedScale; // 복귀 중에도 스케일 고정
            yield return null;
        }

        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        transform.localScale = FixedScale;

        SlideProgress = 0f;
        EvaluatePumpEvents();
    }

    void EvaluatePumpEvents()
    {
        bool isBack = SlideProgress >= backThreshold;
        bool isForward = SlideProgress <= forwardThreshold;

        if (isBack && !reachedBack)
        {
            reachedBack = true;
            cycledOnce = true;
            OnPumpBack?.Invoke();
        }
        else if (!isBack && reachedBack && SlideProgress < backThreshold - 0.05f)
        {
            reachedBack = false;
        }

        if (isForward && !reachedForward)
        {
            reachedForward = true;
            OnPumpForward?.Invoke();

            if (cycledOnce)
            {
                cycledOnce = false;
                OnPumpCycleComplete?.Invoke();
            }
        }
        else if (!isForward && reachedForward && SlideProgress > forwardThreshold + 0.05f)
        {
            reachedForward = false;
        }
    }
}
