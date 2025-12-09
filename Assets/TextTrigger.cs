using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetCanvas;
    [SerializeField] private string playerTag = "Player";

    private void Awake() => SetCanvasActive(false);

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        SetCanvasActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        SetCanvasActive(false);
    }

    private bool IsPlayer(Collider other) => other.CompareTag(playerTag);

    private void SetCanvasActive(bool isActive)
    {
        if (targetCanvas == null) return;
        targetCanvas.SetActive(isActive);
    }
}
