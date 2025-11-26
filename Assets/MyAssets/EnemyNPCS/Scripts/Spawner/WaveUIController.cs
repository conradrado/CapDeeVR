using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI hook for WaveSpawner events and manual wave start.
/// </summary>
public class WaveUIController : MonoBehaviour
{
    [SerializeField] WaveSpawner spawner;
    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text aliveText;
    [SerializeField] GameObject nextWavePanel;
    [SerializeField] Button startWaveButton; // hook this to StartNextWave

    void OnEnable()
    {
        if (spawner == null) return;
        spawner.onWaveStarted.AddListener(OnWaveStarted);
        spawner.onAliveCountChanged.AddListener(OnAliveChanged);
        spawner.onWaveCompleted.AddListener(OnWaveCompleted);
    }

    void OnDisable()
    {
        if (spawner == null) return;
        spawner.onWaveStarted.RemoveListener(OnWaveStarted);
        spawner.onAliveCountChanged.RemoveListener(OnAliveChanged);
        spawner.onWaveCompleted.RemoveListener(OnWaveCompleted);
    }

    public void StartNextWave()
    {
        if (spawner != null)
        {
            spawner.StartNextWave();
            if (nextWavePanel != null)
                nextWavePanel.SetActive(false);
        }
    }

    void OnWaveStarted(int waveIndex)
    {
        if (waveText != null)
            waveText.text = $"Wave {waveIndex}";
    }

    void OnWaveCompleted(int waveIndex)
    {
        if (nextWavePanel != null)
            nextWavePanel.SetActive(true);
    }

    void OnAliveChanged(int count)
    {
        if (aliveText != null)
            aliveText.text = $"남은 적: {count}";
    }
}
