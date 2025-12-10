using UnityEngine;
using UnityEngine.SceneManagement;

public class ScalePuzzlePortal : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName = "WeightPuzzle";

    // XR Simple Interactable의 이벤트에서 이 함수를 호출하게 만들 거야.
    public void LoadScalePuzzle()
    {
        SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
    }
}
