using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Spawn/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Tooltip("Time (seconds) to wait before this wave begins spawning.")]
    public float StartDelay = 0f;

    public WaveEntry[] Entries;
}

[System.Serializable]
public class WaveEntry
{
    public GameObject EnemyPrefab;
    [Min(1)] public int Count = 1;
    [Tooltip("Delay between individual spawns in this entry.")]
    public float SpawnInterval = 0.4f;
}
