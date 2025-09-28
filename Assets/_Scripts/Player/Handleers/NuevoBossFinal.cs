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

        PerformAttack();
    }
}
