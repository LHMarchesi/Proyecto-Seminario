using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RainObjBossAttack", menuName = "BossAttacks/RainObjAttack")]
public class RainProjectilesAttack : BossAttackStats
{
    [Header("Projectiles Settings")]
    public GameObject[] projectilePrefabs;
    public int projectilesPerWave;
    public float spawnInterval;
    public float spawnHeight;
    public float spawnRadius;


    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        Debug.Log("Executed on " + name + " in " + boss.name );
        boss.StartCoroutine(SpawnRainRoutine(target));
    }

    private IEnumerator SpawnRainRoutine(Transform target)
    {
        if (attackDelay > 0f)
            yield return new WaitForSeconds(attackDelay);

        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            for (int i = 0; i < projectilesPerWave; i++)
                SpawnProjectile(target.position);

            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }
    }

    private void SpawnProjectile(Vector3 targetCenter)
    {
        if (projectilePrefabs.Length == 0) return;

        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(
            targetCenter.x + randomCircle.x,
            targetCenter.y + spawnHeight,
            targetCenter.z + randomCircle.y
        );

        GameObject prefab = projectilePrefabs[UnityEngine.Random.Range(0, projectilePrefabs.Length)];
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}