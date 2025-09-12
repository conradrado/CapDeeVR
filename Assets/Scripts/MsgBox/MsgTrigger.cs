using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MsgTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager; // XR ORIGIN(PLAYER)에 붙어있는 dialogueManager 컴포넌트를 매개변수로 한다.
    public Texture2D NPCImage; // 메세지에 표시할 NPC나 사물의 이미지를 매개변수로 한다.
    public string messageToShow; // 표기하고 싶은 메세지를 STRING으로 받는다.

    // 트리거와 플레이어가 충돌(인식)할 때 메세지 박스를 표시하는 함수
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 트리거와 플레이어가 충돌 시 
        {
            Debug.Log("Player msgBoxHit");
            dialogueManager.ShowDialogue(messageToShow,NPCImage); // dialogueManger 스크립트의 ShowDialogue 실행.
        }
    }
    
    // 플레이어가 트리거의 범위에서 벗어났을 때 (trigger exit)
    void OnTriggerExit(Collider other){
        if(other.CompareTag("Player")) // 
        {
            Debug.Log("Player msgBoxExit");
            dialogueManager.HideDialogue();
        }


    }
    
}
