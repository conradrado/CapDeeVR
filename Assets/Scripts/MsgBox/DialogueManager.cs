using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    
    public GameObject dialogueBox; // 플레이어가 보는 메세지 (대화) 박스
    public TextMeshProUGUI dialogueText; // 대화 박스에서 디스플레이 할 text 객체
    public float typingSpeed = 0.02f; // 글자가 타이핑 되는 속도
    public RawImage NPCRawImg;

    private Coroutine typingCoroutine;

    // 메세지(대화)를 보여주는 함수
    public void ShowDialogue(string message, Texture2D NPCImage) // 메세지와 NPC 이미지를 매개변수로 받는다.
    {   
        if (typingCoroutine != null) // 코루틴이 돌아가지 않으면 NULL
            return;
    
        dialogueBox.SetActive(true); 
        NPCRawImg.gameObject.SetActive(true);
        NPCRawImg.texture = NPCImage;

        typingCoroutine = StartCoroutine(TypeSentence(message)); // typingCoroutine을 활성화? 
    }

    
    IEnumerator TypeSentence(string sentence) // 문장을 매개변수로 받는다.
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        yield return new WaitForSeconds(1);
        typingCoroutine = null;
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);
    }
}
