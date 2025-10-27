using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemySpawner : MonoBehaviour
{
    [Header("Pooling")]
    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Spawn Control")]
    [SerializeField] private float spawnRate = 1f; // enemigos por segundo
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Flocking")]
    [SerializeField] private bool enableFlocking = true;
    [SerializeField] public float cohesionWeight = 1f;
    [SerializeField] public float separationWeight = 1.5f;
    [SerializeField] public float alignmentWeight = 1f;
    [SerializeField] public float neighborRadius = 3f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    void Start()
    {
        CreatePool();
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
        while (true)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(1f / spawnRate); // delay entre enemigos
            }

            // Espera antes de la siguiente ola
            yield return new WaitForSeconds(3f);
        }
    }

    void SpawnEnemy()
    {
        if (pool.Count == 0 || spawnPoints.Length == 0) return;

        GameObject enemy = pool.Dequeue();
        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
        baseEnemy.Initialize(this);
        baseEnemy.Spawn(spawnPoints[Random.Range(0, spawnPoints.Length)]);
        enemy.SetActive(true);
        activeEnemies.Add(enemy);

        var flock = enemy.GetComponent<FlockingBehavior>();
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
}
