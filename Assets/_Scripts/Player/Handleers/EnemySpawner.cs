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

    [Header("Dificultad")]
    [SerializeField] private float healthIncreasePerWave = 5f;
    [SerializeField] private float damageIncreasePerWave = 1f;
    [SerializeField] private float speedIncreasePerWave = 0.2f;
    [SerializeField] private float attackSpeedIncreasePerWave = 0.1f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    private int currentWave = 0;

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
            currentWave++;
            Debug.Log($"--- Iniciando ola {currentWave} ---");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(1f / spawnRate);
            }

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

        // Aplicar dificultad escalada
        ApplyDifficultyScaling(baseEnemy); //DE MOMENTO SUBE LA DIFICULTAD CADA VEZ QUE SPAWNEA

        // Flocking
        var flock = enemy.GetComponent<FlockingBehavior>();
        if (flock != null && enableFlocking)
            flock.Initialize(this, cohesionWeight, separationWeight, alignmentWeight, neighborRadius);
    }

    void ApplyDifficultyScaling(BaseEnemy enemy)
    {
        float healthBonus = healthIncreasePerWave * (currentWave - 1);
        float damageBonus = damageIncreasePerWave * (currentWave - 1);
        float speedBonus = speedIncreasePerWave * (currentWave - 1);
        float attackSpeedBonus = attackSpeedIncreasePerWave * (currentWave - 1);

        enemy.AddMaxHealth(healthBonus);
        enemy.AddMaxAttackDamage(damageBonus);
        enemy.AddMaxSpeed(speedBonus);
        enemy.AddAttackSpeed(attackSpeedBonus);

        Debug.Log($"Enemy scaled for wave {currentWave}: +{healthBonus} HP, +{damageBonus} DMG, +{speedBonus} SPD, +{attackSpeedBonus} ATKSPD");
    }

    public List<GameObject> GetActiveEnemies() => activeEnemies;

    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
        pool.Enqueue(enemy);
    }
}
