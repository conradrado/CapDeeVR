using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;   

public class WeightOrderSlot : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;                 // ���� ������Ʈ�� �ٿ��� OK
    public WeightItem CurrentItem { get; private set; }

    public System.Action OnChange;                    // ���� �ʿ��� ����

    void Awake()
    {
        if (!socket) socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSelectEntered);
        socket.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        CurrentItem = args.interactableObject.transform.GetComponent<WeightItem>();
        OnChange?.Invoke();
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        CurrentItem = null;
        OnChange?.Invoke();
    }
}
