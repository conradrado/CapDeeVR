using UnityEngine;
using TMPro;

public class KeypadInput : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public string correctPassword = "3145";
    private string input = "";

    public GameObject doorBlocker;   // 문 막는 큐브

    public void AddDigit(string digit)
    {
        if (input.Length < 4)
        {
            input += digit;
            displayText.text = input;
        }
    }

    public void ClearInput()
    {
        input = "";
        displayText.text = "";
    }

    public void Enter()
    {
        if (input == correctPassword)
        {
            displayText.text = "Correct!";

            // 상자 비활성화 (문이 열리도록)
            if (doorBlocker != null)
                doorBlocker.SetActive(false);
        }
        else
        {
            displayText.text = "Wrong!";
        }

        input = "";
    }
}

