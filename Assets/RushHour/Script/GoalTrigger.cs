using UnityEngine;
using TMPro;

public class GoalTrigger : MonoBehaviour
{
    public GameObject clearMessageUI;  // UI 연결용

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCar"))
        {
            Debug.Log("클리어!");
            clearMessageUI.SetActive(true); // UI 표시
        }
    }
}

