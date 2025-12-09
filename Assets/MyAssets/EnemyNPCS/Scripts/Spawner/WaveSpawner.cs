using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] List<WaveConfig> waves = new List<WaveConfig>();
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform defendObject;
    [SerializeField] float interWaveDelay = 3f;
    [SerializeField] bool waitForWaveClear = true;
    [SerializeField] float waveClearTimeout = 0f; // 0 = no timeout
    [SerializeField] bool loopWaves = false;
    [SerializeField] bool playOnStart = true;
    [SerializeField] bool manualAdvance = false; // true: wait for UI to start each wave
    [Header("Completion")]
    [SerializeField] Animator completionAnimator;
    [SerializeField] string completionTrigger;
    [Header("Events")]
    public UnityEvent<int> onWaveStarted = new UnityEvent<int>();
    public UnityEvent<int> onWaveCompleted = new UnityEvent<int>();
    public UnityEvent<int> onAliveCountChanged = new UnityEvent<int>();
    public UnityEvent onAllWavesCompleted = new UnityEvent();

    Coroutine _runner;
    readonly List<GameObject> _alive = new List<GameObject>();
    bool _isRunning;
    int _currentWaveIndex = 0;

    public bool IsRunning => _isRunning;
    public int AliveCount => _alive.Count;
    public int CurrentWaveNumber => _currentWaveIndex + 1;
    public bool HasMoreWaves => loopWaves || _currentWaveIndex < waves.Count;

    void Start()
    {
        if (playOnStart && waves.Count > 0)
        {
            if (manualAdvance)
                StartNextWave(resetIndex: true);
            else
                _runner = StartCoroutine(RunWaves());
        }
    }

    public void Play()
    {
        if (manualAdvance)
        {
            StartNextWave(resetIndex: true);
        }
        else
        {
            if (_runner != null)
                StopCoroutine(_runner);
            _runner = StartCoroutine(RunWaves());
        }
    }

    IEnumerator RunWaves()
    {
        _isRunning = true;
        _currentWaveIndex = 0;

        do
        {
            for (int i = 0; i < waves.Count; i++)
            {
                var wave = waves[i];
                if (wave == null)
                    continue;

                if (wave.StartDelay > 0f)
                    yield return new WaitForSeconds(wave.StartDelay);

                _currentWaveIndex = i;
                onWaveStarted?.Invoke(i + 1);

                yield return StartCoroutine(SpawnWave(wave));

                if (waitForWaveClear)
                    yield return WaitForWaveClear();

                onWaveCompleted?.Invoke(i + 1);

                if (interWaveDelay > 0f)
                    yield return new WaitForSeconds(interWaveDelay);
            }
        }
        while (loopWaves);

        HandleAllWavesCompleted();
        _isRunning = false;
    }

    public void StartNextWave(bool resetIndex = false)
    {
        if (_isRunning)
            return;

        if (resetIndex)
            _currentWaveIndex = 0;

        if (waves.Count == 0)
            return;

        if (_currentWaveIndex >= waves.Count)
        {
            if (loopWaves)
                _currentWaveIndex = 0;
            else
                return;
        }

        _runner = StartCoroutine(RunSingleWave(_currentWaveIndex));
    }

    IEnumerator RunSingleWave(int waveIndex)
    {
        _isRunning = true;

        var wave = waves[waveIndex];
        if (wave != null)
        {
            if (wave.StartDelay > 0f)
                yield return new WaitForSeconds(wave.StartDelay);

            onWaveStarted?.Invoke(waveIndex + 1);

            yield return StartCoroutine(SpawnWave(wave));

            if (waitForWaveClear)
                yield return WaitForWaveClear();

            onWaveCompleted?.Invoke(waveIndex + 1);
        }

        _currentWaveIndex = waveIndex + 1;
        if (loopWaves && _currentWaveIndex >= waves.Count)
            _currentWaveIndex = 0;
        else if (!loopWaves && _currentWaveIndex >= waves.Count)
            HandleAllWavesCompleted();

        _isRunning = false;
    }

    IEnumerator SpawnWave(WaveConfig wave)
    {
        if (wave.Entries == null)
            yield break;

        foreach (var entry in wave.Entries)
        {
            if (entry == null || entry.EnemyPrefab == null || entry.Count <= 0)
                continue;

            for (int i = 0; i < entry.Count; i++)
            {
                SpawnOne(entry.EnemyPrefab);

                if (entry.SpawnInterval > 0f)
                    yield return new WaitForSeconds(entry.SpawnInterval);
            }
        }
    }

    void SpawnOne(GameObject prefab)
    {
        var point = ChooseSpawnPoint();
        if (point == null)
        {
            Debug.LogWarning("[WaveSpawner] No spawn points assigned.");
            return;
        }

        Vector3 pos = point.position;
        Quaternion rot = point.rotation;

        var go = Instantiate(prefab, pos, rot);
        _alive.Add(go);
        RaiseAliveCount();

        // Hook defend target into AI
        var detect = go.GetComponent<EnemyDetect>();
        detect?.SetDefendObject(defendObject);

        // Ensure NavMeshAgent starts at spawn and moves toward the defend object
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(pos);
            if (defendObject != null)
                agent.SetDestination(defendObject.position);
        }
    }

    Transform ChooseSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        int idx = Random.Range(0, spawnPoints.Length);
        return spawnPoints[idx];
    }

    IEnumerator WaitForWaveClear()
    {
        float timer = 0f;
        while (true)
        {
            int removed = _alive.RemoveAll(item => item == null);
            if (removed > 0)
                RaiseAliveCount();
            if (_alive.Count == 0)
                yield break;

            if (waveClearTimeout > 0f)
            {
                timer += Time.deltaTime;
                if (timer >= waveClearTimeout)
                {
                    Debug.LogWarning("[WaveSpawner] Wave clear wait timed out; proceeding to next wave.");
                    yield break;
                }
            }
            yield return null;
        }
    }

    void HandleAllWavesCompleted()
    {
        onAllWavesCompleted?.Invoke();

        if (completionAnimator != null && !string.IsNullOrEmpty(completionTrigger))
            completionAnimator.SetTrigger(completionTrigger);
    }

    void RaiseAliveCount() => onAliveCountChanged?.Invoke(_alive.Count);
}
