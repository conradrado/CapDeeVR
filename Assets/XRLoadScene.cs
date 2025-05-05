using UnityEngine;
using UnityEngine.SceneManagement;

public class XRLoadScene : MonoBehaviour
{
    public string sceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void LoadScene(){
        Debug.Log("버튼 클릭댐ㅇ요");
        SceneManager.LoadScene(sceneName);
    }
}
