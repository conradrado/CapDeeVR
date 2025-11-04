using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class DeathManager : MonoBehaviour
{

    [Header("💀 References")]
    [Tooltip("페이드용 Plane (검은색 Quad, Main Camera 자식)")]
    public Renderer fadePlane;                    // 검정 Plane (Unlit/Transparent)
    [Tooltip("게임오버 UI CanvasGroup (World Space Canvas)")]
    public CanvasGroup deathCanvas;               // "You Died" Canvas

    [Header("🎮 XR References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRay;              // 오른손 Ray Interactor

    private Material fadeMat;
    private bool isGameOver = false;

    void Awake()
    {
        if (fadePlane != null)
            fadeMat = fadePlane.material;

        // 시작 시 Ray 비활성화 (죽었을 때만 나오도록)
        if (rightRay)
            rightRay.enabled = false;
    }

    /// <summary>
    /// TargetEntity에서 호출됨 (플레이어 사망 시)
    /// </summary>
    public void ShowDeathUI()
    {
        Debug.Log("ShowDeathUI 실행.");
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(FadeOutAndShowUI());
    }

    private IEnumerator FadeOutAndShowUI()
    {
        Debug.Log("요1");
        // 1️⃣ 페이드아웃
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            if (fadeMat != null)
            {
                Color c = fadeMat.color;
                c.a = Mathf.Lerp(0, 1, t);
                fadeMat.color = c;
            }
            yield return null;
        }

        // 2️⃣ "You Died" UI 페이드인
        if (deathCanvas)
        {
            deathCanvas.gameObject.SetActive(true); // 비활성 상태였다면 켜기
            deathCanvas.alpha = 0f;                 // 알파 0에서 시작
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            if (deathCanvas)
                deathCanvas.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        // UI가 보이도록 페이드용 Quad 비활성화
        if (fadePlane)
            fadePlane.enabled = false;

        // 3️⃣ 오른손 Ray 활성화 (죽었을 때만 켜짐)
        if (rightRay)
            rightRay.enabled = true;

        Debug.Log(" Death UI 활성화 + Right Ray 켜짐");
    }

    /// <summary>
    /// UI 버튼 OnClick에서 연결됨 (Restart 버튼)
    /// </summary>
    public void RestartScene()
    {
        // XR Ray 비활성화 후 씬 리로드
        if (rightRay)
            rightRay.enabled = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
