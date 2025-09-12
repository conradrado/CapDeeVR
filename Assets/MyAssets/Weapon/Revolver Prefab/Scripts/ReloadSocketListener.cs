using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReloadSocketListener : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    [SerializeField] private GunFire gunFire;
    [SerializeField] private Transform cylinderAttachPoint; // 실린더 붙일 위치
    [SerializeField] private Reload reload; // Reload 스크립트와 연동이 필요함.

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnSocketInserted);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSocketInserted);
    }

    private void OnSocketInserted(SelectEnterEventArgs args)
    {

        // 1. 재장전
        gunFire.Reload();
        Debug.Log("붙여졌따!");

        // 2. XR 손에서 강제 놓기
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
        {
            if (interactor.hasSelection)
            {
                interactor.interactionManager.SelectExit(interactor, args.interactableObject);
            }
        }

        // 3. 붙일 오브젝트 Transform 가져오기
        Transform insertedObj = args.interactableObject.transform; // 이 경우에는 리볼버 실린더가 insertedObj
        Debug.Log("삽입된 오브젝트: " + insertedObj.name);


        // 4. 부모를 AttachPoint로 바꾸고 위치 정렬
        reload.SetCurrentCylinder(insertedObj); // reload 스크립트에 접근하여 현재 끼워진 실린더를 배출할 수 있도록 insertedObj를 cylinderToEject로 설정
        insertedObj.SetParent(cylinderAttachPoint, worldPositionStays: false); // ✅ 핵심! 
        insertedObj.localPosition = Vector3.zero;
        insertedObj.localRotation = Quaternion.identity;
        insertedObj.localScale = Vector3.one;


        // 5. Rigidbody 세팅
        if (insertedObj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 6. XR Grab 비활성화 (더 이상 손으로 못 뽑게)
        if (insertedObj.TryGetComponent(out UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab))
        {
            grab.enabled = false;
        }


        // 8. Collider 트리거화 (불필요한 충돌 방지)
        if (insertedObj.TryGetComponent(out Collider col))
        {
            col.isTrigger = true;
        }

        Debug.Log("실린더 삽입 완료 및 재장전됨!");
    }
}
