using System.Collections;
using UnityEngine;

public class FallingWithHammer : PlayerState
{
    Vector3 initialDir;

    public FallingWithHammer(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("AirAttack");
        // Dirección tunable
        initialDir =
                        Vector3.down * playerContext.PlayerController.playerStats.downwardMultiplier;
    }

    public override void Update()
    {
        if (playerContext.PlayerController.IsGrounded())
        {
            DoGroundImpact();
            stateMachine.ResetAnimations();
        }
        else
        {
            Rigidbody rb = playerContext.PlayerController.GetRigidbody();
            rb.velocity = Vector3.zero;
            rb.AddForce(initialDir.normalized * playerContext.PlayerController.playerStats.slamForce, ForceMode.VelocityChange);
        }
    }

    private void DoGroundImpact()
    {
        // radio del golpe
        float radius = 20f;
        float damage = 100f;

        Vector3 impactPoint = new Vector3(playerContext.PlayerController.transform.position.x, playerContext.PlayerController.transform.position.y, playerContext.PlayerController.transform.position.z - 2f);

        // buscar enemigos en el ?rea
        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, radius);
        foreach (var hit in hitColliders)
        {
            if (hit.gameObject == playerContext.PlayerController.gameObject)
                continue;

            IDamageable dmg = hit.GetComponent<IDamageable>();
            if (dmg != null)
            {

                dmg.TakeDamage(damage);
            }
        }

        GameObject.Instantiate(playerContext.smashVFX, impactPoint, Quaternion.identity);
        CameraManager.Instance.DoScreenShake(0.1f, 0.3f);
    }
}
