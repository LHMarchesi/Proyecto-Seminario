using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SweepLineAttack", menuName = "BossAttacks/SweepLineAttack")]
public class SweepLineAttack : BossAttackStats
{
    [Header("Sweep Settings")]
    public GameObject sweepPrefab;      // El cubo alargado que barre
    public float sweepSpeed;            // Velocidad de rotación en grados por segundo
    public float sweepHeight;           // Altura sobre el piso

    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        Debug.Log("SweepLineAttack ejecutado en " + boss.name);
        boss.StartCoroutine(SweepRoutine(boss));
    }

    private IEnumerator SweepRoutine(BossEnemy boss)
    {
        if (attackDelay > 0f)
            yield return new WaitForSeconds(attackDelay);

        // Posición del sweep sobre el boss
        Vector3 spawnPos = boss.transform.position;
        spawnPos.y = sweepHeight;

        // Instanciar el sweepPrefab
        GameObject sweepObj = Instantiate(sweepPrefab, spawnPos, Quaternion.identity);

        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            // Rotar alrededor del eje Y local
            sweepObj.transform.Rotate(Vector3.up, sweepSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(sweepObj);
    }
}
