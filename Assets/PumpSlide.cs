using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PumpSlide : MonoBehaviour
{
    [Header("슬라이드 범위 (총의 자식 기준)")]
    public Transform slideStart;   // 펌프 전진 한계점
    public Transform slideEnd;     // 펌프 후퇴 한계점
    [Header("총기 본체 Transform (Shotgun)")]
    public Transform referenceRoot; // Shotgun Transform
    public float returnSpeed = 5f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    private float grabOffsetZ;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    // ✅ 항상 (1,1,1)로 유지할 기준 스케일
    private static readonly Vector3 FixedScale = Vector3.one;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void Start()
    {
        if (referenceRoot == null)
        {
            referenceRoot = transform.parent;
            Debug.LogWarning($"[PumpSlide] referenceRoot가 비어있어서 부모({referenceRoot.name})로 자동 설정됨.");
        }

        // 최초에도 스케일을 (1,1,1)로 고정
        transform.localScale = FixedScale;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

        // 부모를 유지하되, 스케일은 강제로 (1,1,1)
        transform.SetParent(referenceRoot, true);
        transform.localScale = FixedScale;

        // 손의 현재 위치를 총 기준 로컬좌표로 변환
        Vector3 handLocal = referenceRoot.InverseTransformPoint(interactor.transform.position);
        grabOffsetZ = handLocal.z - transform.localPosition.z;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;

        // 부모 재설정 및 스케일 복원
        transform.SetParent(referenceRoot, true);
        transform.localScale = FixedScale;

        StopAllCoroutines();
        StartCoroutine(ReturnToOrigin());
    }

    void LateUpdate()
    {
        // 혹시 부모가 0.12로 바뀌더라도 매 프레임 스케일 고정
        transform.localScale = FixedScale;

        if (interactor == null) return;

        Vector3 handLocal = referenceRoot.InverseTransformPoint(interactor.transform.position);
        float zMin = Mathf.Min(slideStart.localPosition.z, slideEnd.localPosition.z);
        float zMax = Mathf.Max(slideStart.localPosition.z, slideEnd.localPosition.z);

        float newZ = Mathf.Clamp(handLocal.z - grabOffsetZ, zMin, zMax);
        transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, newZ);
        transform.localRotation = initialLocalRot;
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
            transform.localScale = FixedScale; // 복귀 중에도 스케일 유지
            yield return null;
        }

        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        transform.localScale = FixedScale;
    }
}
