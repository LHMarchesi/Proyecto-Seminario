using System;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] Positions;
    [SerializeField] private float timeBtwSpawns;
    private float timeSinceSpawn;

    [SerializeField] private BaseEnemy[] enemyPrefabs;
    [SerializeField] private IObjectPool<BaseEnemy> enemyPool;
    private bool canSpawn = true;

    private void Awake()
    {
        enemyPool = new ObjectPool<BaseEnemy>(CreateEnemy, actionOnGet, actionOnRelease);
    }

    private void actionOnRelease(BaseEnemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void actionOnGet(BaseEnemy enemy)
    {
        enemy.gameObject.SetActive(true);
        Transform randomPos = Positions[UnityEngine.Random.Range(0, Positions.Length)];
        enemy.transform.position = randomPos.position;
    }

    private BaseEnemy CreateEnemy()
    {
        // Selecciona un prefab aleatorio del array
        BaseEnemy rndPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        BaseEnemy enemy = Instantiate(rndPrefab);
        enemy.SetPool(enemyPool);
        return enemy;
    }

    private void Update()
    {
        if (Time.time >= timeSinceSpawn && canSpawn)
        {
            enemyPool.Get();
            timeSinceSpawn = Time.time + timeBtwSpawns; // Actualiza el "siguiente spawn"
        }
    }

}