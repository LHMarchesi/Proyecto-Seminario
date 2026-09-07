using System.Collections;
using UnityEngine;

public class FallingWithHammer : PlayerState
{
    private Vector3 initialDir;
    private bool hasLanded;

    public FallingWithHammer(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        hasLanded = false;
        playerContext.HandleAnimations.ChangeAnimationState("AirAttack");
        initialDir = Vector3.down * playerContext.PlayerController.playerStats.downwardMultiplier;
    }

    public override void Update()
    {
        if (hasLanded) return;
        if (playerContext.PlayerController.IsGrounded())
        {
            hasLanded = true;
            DoGroundImpact();
            stateMachine.ResetAnimations();
            return;
        }
        Rigidbody rb = playerContext.PlayerController.GetRigidbody();
        rb.velocity = Vector3.zero;
        rb.AddForce(initialDir.normalized * playerContext.PlayerController.playerStats.slamForce,
            ForceMode.VelocityChange);
    }

    private void DoGroundImpact()
    {
        // Conservamos los valores del prototipo. No hay desplazamiento artificial en Z.
        float baseRadius = playerContext.HandleAttack.FallingBaseRadius;
        float baseDamage = playerContext.HandleAttack.FallingBaseDamage;
        Vector3 impactPoint = playerContext.HandleAttack.GetGroundImpactPoint(
            playerContext.PlayerController.transform.position);

        // El golpe base mantiene TakeDamage y su hit reaction. Un enemigo se golpea una vez.
        foreach (BaseEnemy enemy in CombatAreaDamage.FindTargets(impactPoint, baseRadius,
            playerContext.HandleAttack.EnemyHitLayer))
        {
            if (enemy != null && !enemy.IsDead()) enemy.TakeDamage(baseDamage);
        }

        if (playerContext.smashVFX != null)
        {
            GameObject vfx = Object.Instantiate(playerContext.smashVFX, impactPoint, Quaternion.identity);
            Object.Destroy(vfx, 3f);
        }
        // Se emite después del daño base; la onda adicional no depende de un enemigo vivo.
        playerContext.HandleAttack.NotifyFallingHammerLanding(impactPoint);
        CameraManager.Instance.DoScreenShake(0.1f, 0.3f);
    }
}
