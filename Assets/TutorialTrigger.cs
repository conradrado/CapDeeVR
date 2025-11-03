using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string tutorialID; // 어떤 튜토리얼인지 구분용 (ex. "MoveTutorial")
    [SerializeField] private Texture2D tutorialImg; // 튜토리얼 UI에 띄울 RawImage의 Texture

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("튜토리얼 트리거 충돌");
            TutorialManager.Instance.ShowTutorial(tutorialID, tutorialImg);
            gameObject.SetActive(false); // 재진입 방지
        }
    }
}
