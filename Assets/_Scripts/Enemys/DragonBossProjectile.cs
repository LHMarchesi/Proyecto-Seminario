using UnityEngine;

[DisallowMultipleComponent]
public class DragonBossProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float hitRadius;
    private float remainingLifetime;
    private LayerMask playerMask;
    private float horizontalKnockback;
    private float verticalKnockback;
    private GameObject impactVFX;
    private float impactVFXLifetime;

    private bool initialized;
    private bool impacted;

    private Rigidbody body;

    private readonly Collider[] hitBuffer =
        new Collider[24];

    public void Initialize(
        Vector3 travelDirection,
        float travelSpeed,
        float projectileDamage,
        float radius,
        float lifetime,
        LayerMask players,
        float horizontalForce,
        float verticalForce,
        GameObject impactEffect,
        float impactEffectLifetime)
    {
        direction =
            travelDirection.sqrMagnitude > 0.001f
                ? travelDirection.normalized
                : transform.forward;

        speed = Mathf.Max(0.1f, travelSpeed);
        damage = Mathf.Max(0f, projectileDamage);
        hitRadius = Mathf.Max(0.05f, radius);
        remainingLifetime = Mathf.Max(0.1f, lifetime);

        playerMask = players;
        horizontalKnockback = Mathf.Max(0f, horizontalForce);
        verticalKnockback = Mathf.Max(0f, verticalForce);

        impactVFX = impactEffect;
        impactVFXLifetime = Mathf.Max(0f, impactEffectLifetime);

        body = GetComponent<Rigidbody>();

        // El movimiento del proyectil lo controla este script.
        // Si el prefab ya tenía Rigidbody evitamos gravedad/doble movimiento.
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        transform.rotation =
            Quaternion.LookRotation(direction);

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || impacted)
            return;

        float step =
            speed * Time.deltaTime;

        Vector3 start =
            transform.position;

        Vector3 end =
            start + direction * step;

        if (TryHitPlayer(start, end))
            return;

        transform.position = end;

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
            Destroy(gameObject);
    }

    private bool TryHitPlayer(
        Vector3 start,
        Vector3 end)
    {
        int count =
            Physics.OverlapCapsuleNonAlloc(
                start,
                end,
                hitRadius,
                hitBuffer,
                playerMask,
                QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider col = hitBuffer[i];

            if (col == null)
                continue;

            PlayerController player =
                col.GetComponentInParent<PlayerController>();

            if (player == null)
                continue;

            ImpactPlayer(player);
            return true;
        }

        return false;
    }

    private void ImpactPlayer(
        PlayerController player)
    {
        if (impacted || player == null)
            return;

        impacted = true;

        if (damage > 0f)
            player.TakeDamage(damage);

        Rigidbody playerBody =
            player.GetComponent<Rigidbody>();

        if (playerBody != null &&
            (horizontalKnockback > 0f ||
             verticalKnockback > 0f))
        {
            Vector3 horizontalDirection =
                direction;

            horizontalDirection.y = 0f;

            if (horizontalDirection.sqrMagnitude < 0.001f)
            {
                horizontalDirection =
                    player.transform.position -
                    transform.position;

                horizontalDirection.y = 0f;
            }

            horizontalDirection.Normalize();

            Vector3 force =
                horizontalDirection * horizontalKnockback +
                Vector3.up * verticalKnockback;

            playerBody.AddForce(
                force,
                ForceMode.Impulse);
        }

        SpawnImpactVFX();
        Destroy(gameObject);
    }

    private void SpawnImpactVFX()
    {
        if (impactVFX == null)
            return;

        GameObject vfx =
            Instantiate(
                impactVFX,
                transform.position,
                Quaternion.identity);

        if (impactVFXLifetime > 0f)
            Destroy(vfx, impactVFXLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            hitRadius > 0f ? hitRadius : 0.5f);
    }
}
