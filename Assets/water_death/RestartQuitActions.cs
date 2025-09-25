using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartQuitActions : MonoBehaviour
{
    public void RestartScene()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}

