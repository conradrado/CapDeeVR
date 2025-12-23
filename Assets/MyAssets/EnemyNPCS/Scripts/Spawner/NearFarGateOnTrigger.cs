using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Enables the far-casting portion of a Near-Far Interactor while the player stays inside the trigger.
/// Intended to be placed on a trigger volume and wired to the player's right-hand Near-Far Interactor.
/// </summary>
public class NearFarGateOnTrigger : MonoBehaviour
{
    [SerializeField] NearFarInteractor nearFarInteractor;
    [SerializeField] string playerTag = "Player";

    void Awake()
    {
        // Ensure this collider works as a trigger gate
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        SetFar(false);
    }

    void OnDisable()
    {
        SetFar(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        SetFar(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        SetFar(false);
    }

    void SetFar(bool enabled)
    {
        if (nearFarInteractor != null)
            nearFarInteractor.enableFarCasting = enabled;
    }
}
