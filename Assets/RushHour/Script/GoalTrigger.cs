using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;   // ← 씬 로드용
using System.Collections;            // ← 코루틴용

public class GoalTrigger : MonoBehaviour
{
    public GameObject clearMessageUI;    // 클리어 UI 오브젝트
    public string returnSceneName = "Puzzle";  // 돌아갈 원래 퍼즐맵 씬 이름
    public float returnDelay = 5f;       // 몇 초 뒤에 돌아갈지

    private bool hasCleared = false;     // 중복 실행 방지용

    private void OnTriggerEnter(Collider other)
    {
        if (!hasCleared && other.CompareTag("MainCar"))
        {
            hasCleared = true;

            Debug.Log("클리어!");
            if (clearMessageUI != null)
                clearMessageUI.SetActive(true); // UI 표시

            // 5초 뒤 원래 씬으로 돌아가는 코루틴 시작
            StartCoroutine(ReturnToMainSceneAfterDelay());
        }
    }

    private IEnumerator ReturnToMainSceneAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        SceneManager.LoadScene(returnSceneName);
    }
}

