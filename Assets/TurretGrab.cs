using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TurretGrab : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor CurrentInteractor { get; private set; }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        CurrentInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (CurrentInteractor == args.interactorObject)
            CurrentInteractor = null;
    }
}
