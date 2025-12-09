using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class BoxTrigger : MonoBehaviour
{

    public Animator bxAnimator;
    
    // 트리거와 플레이어가 충돌(인식)할 때 메세지 박스를 표시하는 함수
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 트리거와 플레이어가 충돌 시 
        {
            Debug.Log("Player BoxTrgHit");
            bxAnimator.SetTrigger("OpenBox");
        }
    }
    
    // 플레이어가 트리거의 범위에서 벗어났을 때 (trigger exit)
    void OnTriggerExit(Collider other){
        if(other.CompareTag("Player")) // 
        {
            Debug.Log("Player msgBoxExit");
            bxAnimator.SetTrigger("CloseBox");
        }


    }
    
}
