using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public string sceneToLoad;
    public float fadeDuration = 1f;
    public Image fadeImage; // 拖拽Canvas上的全屏黑色Image进来

    private void Start()
    {
        // 初始为黑色->透明（淡入）
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    public void ChangeScene()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    IEnumerator FadeIn()
    {
        float t = fadeDuration;
        Color color = fadeImage.color;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false); // 透明后隐藏
    }

    IEnumerator FadeOutAndLoadScene()
    {
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color color = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
