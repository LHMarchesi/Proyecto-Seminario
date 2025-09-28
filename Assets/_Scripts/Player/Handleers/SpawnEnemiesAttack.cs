using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnEnemiesAttack", menuName = "BossAttacks/SpawnEnemiesAttack")]
public class SpawnEnemiesAttack : BossAttackStats
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public int enemiesToSpawn;
    public float spawnRadius;
    public float spawnDelay;

    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        boss.StartCoroutine(SpawnRoutine(boss.transform));
    }

    private IEnumerator SpawnRoutine(Transform bossTransform)
    {
        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (enemyPrefabs.Length == 0) break;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = bossTransform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        yield return null;
    }
}