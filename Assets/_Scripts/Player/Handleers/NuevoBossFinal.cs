using UnityEngine;

public class NuevoBossFinal : BossEnemy
{
    protected override void OnEnable()
    {
        base.OnEnable();
        canMove = false; // nunca se mueve
    }

    // Sobrescribimos para evitar que persiga
    protected override void Update()
    {
        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime;
            return;
        }

        if (target == null) return;

        // Debug de rango
        float dist = Vector3.Distance(transform.position, target.position);
        Debug.Log($"[Boss Debug] Distancia al jugador: {dist} | Attack Range: {GetCurrentAttackRange()}");

        // Forzar ataque siempre
        PerformAttack();
    }
}
