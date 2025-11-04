using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.TriggerVictory();
        }
    }
}

