using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemySpawner : MonoBehaviour
{
    [Header("Pooling")]
    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Spawn Control")]
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Total Enemies To Spawn")]
    [SerializeField] private int minEnemiesToSpawn = 10;
    [SerializeField] private int maxEnemiesToSpawn = 20;

    [Header("Flocking")]
    [SerializeField] private bool enableFlocking = true;
    [SerializeField] public float cohesionWeight;
    [SerializeField] public float separationWeight;
    [SerializeField] public float alignmentWeight;
    [SerializeField] public float neighborRadius;

    [Header("Difficulty System")]
    [SerializeField] public float currentDifficulty = 0f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    private int totalEnemiesToSpawn;
    private int enemiesSpawned = 0;

    private ObstacleDestructible destructible; // ← para controlar el daño

    void Start()
    {
        CreatePool();
        totalEnemiesToSpawn = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn + 1);
        destructible = GetComponent<ObstacleDestructible>();

        // Bloquear daño mientras spawnea
        if (destructible != null)
            destructible.SetCanTakeDamage(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isSpawning)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            isSpawning = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isSpawning)
        {
            StopCoroutine(spawnCoroutine);
            isSpawning = false;
        }
    }

    void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], transform);
            enemy.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (enemiesSpawned < totalEnemiesToSpawn)
        {
            for (int i = 0; i < enemiesPerWave && enemiesSpawned < totalEnemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(1f / spawnRate);
            }
            yield return new WaitForSeconds(3f);
        }

        // Cuando termina de spawnear todo:
        if (destructible != null)
            destructible.SetCanTakeDamage(true); // ahora puede recibir daño

        isSpawning = false;
    }

    void SpawnEnemy()
    {
        if (pool.Count == 0 || spawnPoints.Length == 0) return;

        GameObject enemy = pool.Dequeue();
        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();

        baseEnemy.Initialize(this);
        baseEnemy.Spawn(spawnPoints[Random.Range(0, spawnPoints.Length)]);

        baseEnemy.ApplyDifficulty(currentDifficulty);


        enemy.SetActive(true);
        activeEnemies.Add(enemy);
        enemiesSpawned++;

        var flock = enemy.GetComponent<FlockingBehave>();
        if (flock != null && enableFlocking)
            flock.Initialize(this, cohesionWeight, separationWeight, alignmentWeight, neighborRadius);
    }

    public List<GameObject> GetActiveEnemies() => activeEnemies;

    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
        pool.Enqueue(enemy);
    }

    private void OnDestroy()
    {
        NotifyAllSpawnersDifficultyUp();
        SoundManagerOcta.Instance.PlaySound("PilarDestroy");
    }

    private void NotifyAllSpawnersDifficultyUp()
    {

        EnemySpawner[] allSpawners = FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in allSpawners)
        {
            spawner.IncreaseDifficulty(1f);
        }
        Debug.Log($"Spawner {name} murió. Todos los spawners aumentan dificultad +1");
    }

    public void IncreaseDifficulty(float amount)
    {
        currentDifficulty += amount;
    }

    public void RestartDifficulty()
    {
        currentDifficulty = 0;
    }
}
