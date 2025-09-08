using UnityEngine;

public class FallingWithHammer : PlayerState
{
    Vector3 initialDir;

    public FallingWithHammer(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        // Dirección tunable
        initialDir =
             playerContext.PlayerController.transform.forward * playerContext.PlayerController.playerStats.forwardMultiplier +
             Vector3.down * playerContext.PlayerController.playerStats.downwardMultiplier;
    }

    public override void Update()
    {
        if (playerContext.PlayerController.IsGrounded())
        {
            //DoGroundImpact();
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
        float radius = 5f;
        float damage = 20f;

        Vector3 impactPoint = playerContext.PlayerController.transform.position;

        // buscar enemigos en el ?rea
        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, radius);
        foreach (var hit in hitColliders)
        {
            IDamageable dmg = hit.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
            }
        }

        // GameObject.Instantiate(playerContext.impactVFX, impactPoint, Quaternion.identity);
        // CameraManager.Instance.ShakeCamera(0.3f, 0.5f);
    }
}
