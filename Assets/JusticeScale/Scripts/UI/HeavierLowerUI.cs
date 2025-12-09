using UnityEngine;
using UnityEngine.UI;
using JusticeScale.Scripts;   // ScaleController 사용을 위해 필요!

public class HeavierLowerUI : MonoBehaviour
{
    [SerializeField] private ScaleController scaleController;
    [SerializeField] private Text uiText;

    private void Awake()
    {
        // 자동으로 참조 시도
        if (scaleController == null)
            scaleController = FindObjectOfType<ScaleController>();

        if (uiText == null)
            uiText = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        // 왼쪽 총 무게 - 오른쪽 총 무게
        float diff = scaleController.WeightDifference;

        if (diff > 0.1f)
        {
            uiText.text = "Left is Heavier";
        }
        else if (diff < -0.1f)
        {
            uiText.text = "Right is Heavier";
        }
        else
        {
            uiText.text = "Balanced";
        }
    }
}
