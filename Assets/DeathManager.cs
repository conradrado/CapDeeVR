using UnityEngine;

public class DeathManager : MonoBehaviour
{

    public GameObject rstButton;
    public GameObject quitButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void GameOver(){
        Debug.Log("GameOver!!");
        rstButton.SetActive(true);
        quitButton.SetActive(true);
    }

}
