using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPose : MonoBehaviour
{
    // 총을 쥘 때의 손 정보 (손가락들의 위치, Rotation 등..)
    public HandData rightHandPose; 
    public HandData leftHandPose; 

    // 게임이 시작되자마자 실행되는 함수?
    void Start()
    {   
        // XRGrabInteractable(총기)를 가져와서 grabbInteractable이라는 변수에 할당
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();   
        
        // 해당 총기를 grab하면 이벤트 리스너 추가. (SetupPose)
        grabInteractable.selectEntered.AddListener(SetupPose);
        
        // 총기에 붙어있던 손의 gameObject를 비활성화함
        rightHandPose.gameObject.SetActive(false);
        leftHandPose.gameObject.SetActive(false);
        
        // 해당 총기를 놓았을 때의 이벤트 (selectExited) 발생 시 NoGrab 콜백 함수 실행.
        grabInteractable.selectExited.AddListener(NoGrab);

    }

    // 총기가 grab 동작을 인식할 때 실행되는 이벤트 리스너

    // interactorObject = 물체를 grab하고 있는 객체
    // interacableObject = grab 당하고 있는 물체
    // XRDirectInteractor = 컨트롤러 
    public void SetupPose(BaseInteractionEventArgs arg) // 여기서 arg는 grab이 동작할 때의 정보들을 담는 객체라고 합니다.
    {

        Debug.Log(arg.interactorObject.transform.name);
        // arg 매개변수에 접근하여 현재 총기를 쥐고 있는 interactor의 HandData를 추출. 
        // arg의 interactorObject는 유니티의 API와 호환이 되지 않으므로, 
        // transform 속성을 통해 GetComponent를 실행해야 함.
        HandData interactorData = arg.interactorObject.transform.GetComponent<HandData>();
        
        // 만약 interactorObject의 HandData가 null이면 로그 출력력
        if (interactorData == null){
            Debug.LogWarning("HandData not found.");
            return;
        }
        
        if (interactorData.animator == null){
            Debug.Log("Animator not detected");
        }
        else{
            interactorData.animator.enabled = false;
        }
           

        Debug.Log("Model Type: " + interactorData.modelType);

        // 만약 물체를 잡은 인터랙터가 컨트롤러면 if문 실행
        Debug.Log("Model Type: " + interactorData.modelType);

        if (interactorData.modelType == HandData.HandModelType.left)
        {
            Debug.Log("LeftHand On");
            leftHandPose.gameObject.SetActive(true);
        }
        else if (interactorData.modelType == HandData.HandModelType.right)
        {
            Debug.Log("RightHand On");
            rightHandPose.gameObject.SetActive(true);
        }


        /*if(arg.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor){
            Debug.Log("Yay ");
            HandData handData = arg.interactorObject.transform.GetComponentInChildren<HandData>(); // 현재 쥐고 있는 손의 정보를 가져와서 handData에 할당함.
            
            handData.animator.enabled = false; // 기본 손 애니메이션을 비활성화하여 그랩 도중 독자적인 애니메이션을 구현할 수 있도록 함.
        }
        else if (arg.interactorObject is not UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor){
            Debug.Log("nope^^");
        }*/
    }

    public void NoGrab(BaseInteractionEventArgs arg){
        rightHandPose.gameObject.SetActive(false);
        leftHandPose.gameObject.SetActive(false);
    }
}
