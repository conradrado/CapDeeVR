using UnityEngine;
using TMPro; // TextMeshPro ��� ��
using UnityEngine.UI;

public class KeypadInput : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public string correctPassword = "1234";
    private string input = "";

    public SafeDoor safeDoor; 
    public Animator doorAnimator;

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
            doorAnimator.SetTrigger("OpenDoor");
        }
        else
        {
            displayText.text = "Wrong!";
        }

        input = "";
    }
}
