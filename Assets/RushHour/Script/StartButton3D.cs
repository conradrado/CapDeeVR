using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartButton3D : MonoBehaviour
{
    public CameraManager cameraManager;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnPressed);
    }

    private void OnPressed(SelectEnterEventArgs args)
    {
        if (cameraManager != null)
        {
            cameraManager.MoveToPuzzleView();
        }

        // ��ư ��Ȱ��ȭ (���ϴ� ���)
        gameObject.SetActive(false);
    }
}

