using UnityEngine;
using UnityEngine.SceneManagement;

public class RushHourStartLoader : MonoBehaviour
{
    [SerializeField] private string rushHourSceneName = "Rush Hour";

    public void LoadRushHourScene()
    {
        SceneManager.LoadScene(rushHourSceneName);
    }
}
