using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GameClearTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject gameClearUI;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool pauseTimeOnClear = true;

    private bool _triggered;
    private Collider _collider;
    private Coroutine _fadeRoutine;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null)
            _collider.isTrigger = true;

        if (gameClearUI != null)
            gameClearUI.SetActive(false);

        if (canvasGroup == null && gameClearUI != null)
            canvasGroup = gameClearUI.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag(playerTag))
            return;

        _triggered = true;

        if (gameClearUI != null)
            gameClearUI.SetActive(true);

        if (_collider != null)
            _collider.enabled = false;

        StartFadeIn();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.TriggerVictory();
        else if (pauseTimeOnClear)
            Time.timeScale = 0f;
    }

    void StartFadeIn()
    {
        if (canvasGroup == null)
            return;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeCanvasGroup(0f, 1f));
    }

    private System.Collections.IEnumerator FadeCanvasGroup(float start, float end)
    {
        float elapsed = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = start;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        canvasGroup.alpha = end;
        _fadeRoutine = null;
    }
}
