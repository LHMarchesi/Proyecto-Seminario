using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class WaveSpawner3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TMP_Text waveText;

    [Header("Spawn Settings")]
    public SpawnSource spawnSource = SpawnSource.AroundPlayer;
    public List<Transform> spawnPoints = new List<Transform>();
    public float minSpawnRadius = 15f;
    public float maxSpawnRadius = 25f;
    public float spawnYOffset = 0f;

    [Header("NavMesh")]
    public bool useNavMesh = true;
    public float navMeshMaxDistance = 3f;
    public int navMeshAreaMask = NavMesh.AllAreas;

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();
    public int startWaveIndex = 0;
    public float firstWaveDelay = 2f;
    public bool autoAdvance = true;

    private int currentWaveIndex = -1;
    private int spawnedThisWave;
    private int livingEnemies;
    private float nextSpawnTime;
    private float waveEndTime;
    private bool waveCompleted;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnAllWavesCompleted;

   

    private void Start()
    {
        if (player == null && !string.IsNullOrEmpty(playerTag))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
                player = playerObject.transform;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning($"[{name}] WaveSpawner3D no tiene waves configuradas.");
            enabled = false;
            return;
        }

        startWaveIndex = Mathf.Clamp(startWaveIndex, 0, waves.Count - 1);
        currentWaveIndex = startWaveIndex - 1;

        Invoke(nameof(StartNextWave), firstWaveDelay);
    }

    private void Update()
    {
        if (currentWaveIndex < 0 || currentWaveIndex >= waves.Count)
            return;

        Wave wave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave: {currentWaveIndex + 1}/{waves.Count}";

        if (!wave.spawnAllAtOnce &&
            spawnedThisWave < wave.enemyCount &&
            Time.time >= nextSpawnTime)
        {
            SpawnEnemy(wave);
            nextSpawnTime = Time.time + wave.spawnInterval;
        }

        bool allEnemiesSpawned = spawnedThisWave >= wave.enemyCount;
        bool allEnemiesDead = livingEnemies <= 0;
        bool timerFinished = Time.time >= waveEndTime;

        bool waveFinished = false;

        switch (wave.endCondition)
        {
            case WaveEndCondition.Clear:
                waveFinished = allEnemiesSpawned && allEnemiesDead;
                break;

            case WaveEndCondition.Timer:
                waveFinished = timerFinished;
                break;

            case WaveEndCondition.Spawned:
                waveFinished = allEnemiesSpawned;
                break;
        }

        if (waveFinished && !waveCompleted)
            CompleteWave();
    }

    private void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Count)
        {
            OnAllWavesCompleted?.Invoke();
            return;
        }

        Wave wave = waves[currentWaveIndex];

        spawnedThisWave = 0;
        livingEnemies = 0;
        waveCompleted = false;

        waveEndTime = Time.time + wave.waveDuration;
        nextSpawnTime = Time.time +
                        (wave.spawnAllAtOnce ? 0f : wave.spawnInterval);

        if (wave.spawnAllAtOnce)
        {
            for (int i = 0; i < wave.enemyCount; i++)
                SpawnEnemy(wave);
        }

        OnWaveStarted?.Invoke(currentWaveIndex);
    }

    private void CompleteWave()
    {
        waveCompleted = true;
        OnWaveCompleted?.Invoke(currentWaveIndex);

        if (autoAdvance)
            StartNextWave();
    }

    private void SpawnEnemy(Wave wave)
    {
        if (wave.enemyTypes == null || wave.enemyTypes.Count == 0)
        {
            Debug.LogWarning(
                $"[{name}] La wave {currentWaveIndex + 1} no tiene enemigos configurados."
            );
            return;
        }

        EnemySpawnData selectedEnemy = PickEnemy(wave.enemyTypes);

        if (selectedEnemy == null || selectedEnemy.prefab == null)
        {
            Debug.LogWarning($"[{name}] No se pudo seleccionar un enemigo.");
            return;
        }

        if (!TryGetSpawnPoint(out Vector3 spawnPosition,
                              out Quaternion spawnRotation))
            return;

        GameObject enemyObject = Instantiate(
            selectedEnemy.prefab,
            spawnPosition,
            spawnRotation
        );

        spawnedThisWave++;
        livingEnemies++;

        RegisterEnemyDeath(enemyObject);
    }

    private void RegisterEnemyDeath(GameObject enemyObject)
    {
        if (enemyObject == null)
            return;

        // Compatible con el BaseEnemy de tu proyecto.
        BaseEnemy enemy = enemyObject.GetComponent<BaseEnemy>();
        enemy.Initialize();

        if (enemy != null)
            enemy.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        livingEnemies = Mathf.Max(0, livingEnemies - 1);
    }

    private EnemySpawnData PickEnemy(List<EnemySpawnData> enemies)
    {
        if (enemies == null || enemies.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (EnemySpawnData enemy in enemies)
        {
            if (enemy == null || enemy.prefab == null)
                continue;

            totalWeight += Mathf.Max(0, enemy.weight);
        }

        if (totalWeight <= 0)
            return null;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        foreach (EnemySpawnData enemy in enemies)
        {
            if (enemy == null || enemy.prefab == null)
                continue;

            randomValue -= Mathf.Max(0, enemy.weight);

            if (randomValue < 0)
                return enemy;
        }

        return null;
    }

    private bool TryGetSpawnPoint(
        out Vector3 position,
        out Quaternion rotation)
    {
        if (spawnSource == SpawnSource.FromSpawnPoints)
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning(
                    $"[{name}] FromSpawnPoints está seleccionado pero no hay spawn points."
                );

                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            Transform point = spawnPoints[
                UnityEngine.Random.Range(0, spawnPoints.Count)
            ];

            if (point == null)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }

            position = point.position + Vector3.up * spawnYOffset;
            rotation = point.rotation;

            if (useNavMesh)
                ProjectToNavMesh(ref position);

            return true;
        }

        if (player == null)
        {
            Debug.LogWarning($"[{name}] No hay Player asignado.");

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        float radius = UnityEngine.Random.Range(
            minSpawnRadius,
            maxSpawnRadius
        );

        Vector2 randomCircle =
            UnityEngine.Random.insideUnitCircle.normalized * radius;

        Vector3 candidate =
            player.position +
            new Vector3(randomCircle.x, 0f, randomCircle.y);

        position = candidate + Vector3.up * spawnYOffset;

        Vector3 direction = player.position - position;
        direction.y = 0f;

        rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : Quaternion.identity;

        if (useNavMesh)
            ProjectToNavMesh(ref position);

        return true;
    }

    private bool ProjectToNavMesh(ref Vector3 position)
    {
        if (NavMesh.SamplePosition(
            position,
            out NavMeshHit hit,
            navMeshMaxDistance,
            navMeshAreaMask))
        {
            position = hit.position + Vector3.up * spawnYOffset;
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnSource == SpawnSource.AroundPlayer && player != null)
        {
            Gizmos.color = Color.cyan;
            DrawRing(player.position, minSpawnRadius);

            Gizmos.color = Color.blue;
            DrawRing(player.position, maxSpawnRadius);
        }
    }

    private void DrawRing(Vector3 center, float radius, int segments = 48)
    {
        Vector3 previous =
            center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle =
                (i / (float)segments) * Mathf.PI * 2f;

            Vector3 point =
                center +
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

            Gizmos.DrawLine(previous, point);
            previous = point;
        }
    }
}

public enum SpawnSource
{
    AroundPlayer,
    FromSpawnPoints
}

public enum WaveEndCondition
{
    Timer,
    Clear,
    Spawned
}

[Serializable]
public class Wave
{
    [Header("Enemies")]
    public List<EnemySpawnData> enemyTypes =
        new List<EnemySpawnData>();

    [Header("Count & Timing")]
    [Min(1)]
    public int enemyCount = 10;

    public bool spawnAllAtOnce = false;

    [Min(0f)]
    public float spawnInterval = 0.5f;

    [Min(0f)]
    public float waveDuration = 30f;

    [Header("End Condition")]
    public WaveEndCondition endCondition =
        WaveEndCondition.Clear;
}

[Serializable]
public class EnemySpawnData
{
    [Tooltip("Prefab del enemigo.")]
    public GameObject prefab;

    [Tooltip("Probabilidad relativa de aparición.")]
    [Min(0)]
    public int weight = 50;

}
