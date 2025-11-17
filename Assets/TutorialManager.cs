using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("XR References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRay; // 튜토리얼 중 버튼 상호작용용

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialUIPanel;   // 튜토리얼 전체 UI 오브젝트
    [SerializeField] private TMP_Text tutorialText;         // 텍스트
    [SerializeField] private RawImage tutorialImg;
    [SerializeField] private CanvasGroup canvasGroup;       // 페이드 효과용 (CanvasGroup 추가 필요)

    [Header("Display Settings")]
    [SerializeField] private float distanceFromCamera = 1f; // 카메라 앞 거리
    [SerializeField] private float fadeDuration = 0.4f;     // 페이드 시간

    private bool isTutorialActive = false;
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

        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(false);

        // 시작 시에는 XR Ray는 꺼둠 (튜토리얼 표시 시에만 켬)
        if (rightRay)
            rightRay.enabled = false;
    }

    IEnumerator DelayTime(){
        yield return new WaitForSecondsRealtime(4.0f);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 튜토리얼 UI 표시
    /// </summary>
   public void ShowTutorial(string message, Texture2D img)
{
    if (isTutorialActive) return;
    isTutorialActive = true;

    if (tutorialUIPanel != null)
        tutorialUIPanel.SetActive(true);

    // 메시지 & 이미지 세팅
    tutorialText.text = message;
    tutorialImg.texture = img;

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
        yield return new WaitForSecondsRealtime(1f);

        // 3. 타임스케일 정지
        // Game pause via GameStateManager (fallback to direct)
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.Pause();
        else
            Time.timeScale = 0f;

        // Enable right-hand ray for UI interaction during tutorial
        if (rightRay)
            rightRay.enabled = true;
    }



    /// <summary>
    /// 튜토리얼 UI 닫기
    /// </summary>
    public void CloseTutorial()
    {
        if (!isTutorialActive) return;
        isTutorialActive = false;

        // 페이드 아웃
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration, false));

        // Disable XR Ray after closing
        if (rightRay)
            rightRay.enabled = false;

        // Resume game via GameStateManager
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.Resume();
        else
            Time.timeScale = 1f;
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

        tutorialUIPanel.transform.position = targetPos;
        tutorialUIPanel.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
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
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }

        group.alpha = end;

        if (!setActiveAtEnd)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            tutorialUIPanel.SetActive(false);
        }
    }
}
