using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathManager : MonoBehaviour
{

    public static DeathManager Instance;

    [Header("Death UI")]
    [SerializeField] private GameObject deathUIPanel;   // Death UI 전체 오브젝트
    [SerializeField] private CanvasGroup canvasGroup;


    [Header("Display Settings")]
    [SerializeField] private float distanceFromCamera = 1f; // 카메라 앞 거리
    [SerializeField] private float fadeDuration = 0.4f;     // 페이드 시간

    private bool isDeath = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (deathUIPanel != null)
            deathUIPanel.SetActive(false);
    }

    IEnumerator DelayTime(){
        yield return new WaitForSecondsRealtime(4.0f);
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// 사망 UI 표시
    /// </summary>
    public void ShowDeathUI()
{
    Debug.Log("사망UI");
    if (isDeath) return;
    isDeath = true;

    if (deathUIPanel != null)
        deathUIPanel.SetActive(true);

    // 카메라 앞 위치로 이동
    PositionInFrontOfCamera();

    // 기존 페이드 코루틴 정지 후 새로 실행
    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    fadeCoroutine = StartCoroutine(ShowTutorialSequence());
}

    private IEnumerator ShowTutorialSequence()
{
    // 1. 페이드 인
    yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration, true));

    // 2. 페이드 인 끝난 뒤 4초 대기
    yield return new WaitForSecondsRealtime(4f);

    // 3. 타임스케일 정지
    Time.timeScale = 0.1f;
}

     /// <summary>
    /// HMD 정면에 UI 자동 배치
    /// </summary>
    private void PositionInFrontOfCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        Vector3 targetPos = cam.transform.position + forward * distanceFromCamera;

        deathUIPanel.transform.position = targetPos;
        deathUIPanel.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }


    /// <summary>
    /// CanvasGroup 페이드 인/아웃
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration, bool setActiveAtEnd)
    {
        if (group == null) yield break;

        float time = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }

        group.alpha = end;

        if (!setActiveAtEnd)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            deathUIPanel.SetActive(false);
        }
    }

    // 현재 씬 리로드
    public void RestartGame()
    {
        Debug.Log("리스타트 버튼 클릭됨");
        Time.timeScale = 1f;   // 다시 정상 속도로
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 메인 메뉴 씬으로 이동
    public void QuitGame()
    {
        Debug.Log("종료 버튼 클릭됨");
        Time.timeScale = 1f;   // 시간 정상화
        SceneManager.LoadScene("MainMenu");  // "MainMenu"는 빌드 세팅에 반드시 추가되어야 함
    }

}
