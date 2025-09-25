using UnityEngine;

public class DeathTrigger : MonoBehaviour
{

    public DeathManager dthManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            Debug.Log("Player Deadzone Trigger!");
            dthManager.GameOver();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
