using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Enables a ray interactor only when the player is inside the trigger AND a wave is not running.
/// Use this for pre-round UI interactions (e.g., start round/shop).
/// </summary>
public class RayGateOnRound : MonoBehaviour
{
    [SerializeField] XRRayInteractor ray;
    [SerializeField] WaveSpawner spawner;
    [SerializeField] string playerTag = "Player";

    bool _playerInside;

    void Awake()
    {
        // Default to collider on the same object
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnEnable()
    {
        if (spawner != null)
        {
            spawner.onWaveStarted.AddListener(OnWaveStarted);
            spawner.onWaveCompleted.AddListener(OnWaveCompleted);
        }
    }

    void OnDisable()
    {
        if (spawner != null)
        {
            spawner.onWaveStarted.RemoveListener(OnWaveStarted);
            spawner.onWaveCompleted.RemoveListener(OnWaveCompleted);
        }
        SetRay(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        _playerInside = true;
        UpdateRay();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        _playerInside = false;
        UpdateRay();
    }

    void OnWaveStarted(int waveIndex)
    {
        SetRay(false);
    }

    void OnWaveCompleted(int waveIndex)
    {
        UpdateRay();
    }

    void UpdateRay()
    {
        bool waveRunning = spawner != null && spawner.IsRunning;
        SetRay(_playerInside && !waveRunning);
    }

    void SetRay(bool enabled)
    {
        if (ray != null)
            ray.enabled = enabled;
    }
}
