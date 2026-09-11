using System.Collections;
using UnityEngine;

public class DragonMeteorHazard : MonoBehaviour
{
    private LayerMask playerMask;
    private GameObject warningPrefab;
    private GameObject meteorVisualPrefab;
    private GameObject impactVFXPrefab;

    private float warningTime;
    private float radius;
    private float damage;
    private float meteorHeight;
    private float horizontalKnockback;
    private float verticalKnockback;

    private Vector3 impactPoint;
    private GameObject warningInstance;
    private GameObject meteorVisualInstance;

    private readonly Collider[] hits =
        new Collider[24];

    public void Initialize(
        Vector3 point,
        LayerMask players,
        GameObject warning,
        GameObject meteorVisual,
        GameObject impactVFX,
        float telegraphTime,
        float impactRadius,
        float impactDamage,
        float visualHeight,
        float horizontalForce,
        float verticalForce)
    {
        impactPoint = point;
        playerMask = players;
        warningPrefab = warning;
        meteorVisualPrefab = meteorVisual;
        impactVFXPrefab = impactVFX;

        warningTime = Mathf.Max(0.05f, telegraphTime);
        radius = Mathf.Max(0.1f, impactRadius);
        damage = Mathf.Max(0f, impactDamage);
        meteorHeight = Mathf.Max(0f, visualHeight);
        horizontalKnockback = Mathf.Max(0f, horizontalForce);
        verticalKnockback = Mathf.Max(0f, verticalForce);

        transform.position = impactPoint;

        StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        SpawnWarning();
        SpawnMeteorVisual();

        float elapsed = 0f;

        Vector3 meteorStart =
            impactPoint +
            Vector3.up * meteorHeight;

        Vector3 meteorEnd =
            impactPoint +
            Vector3.up * 0.1f;

        while (elapsed < warningTime)
        {
            float t =
                Mathf.Clamp01(
                    elapsed / warningTime);

            // Empieza lento y acelera al caer.
            float fallT = t * t;

            if (meteorVisualInstance != null)
            {
                meteorVisualInstance.transform.position =
                    Vector3.Lerp(
                        meteorStart,
                        meteorEnd,
                        fallT);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Impact();
    }

    private void SpawnWarning()
    {
        if (warningPrefab == null)
            return;

        warningInstance =
            Instantiate(
                warningPrefab,
                impactPoint +
                Vector3.up * 0.02f,
                Quaternion.identity);

        Vector3 scale =
            warningInstance.transform.localScale;

        float diameter = radius * 2f;

        warningInstance.transform.localScale =
            new Vector3(
                diameter,
                scale.y,
                diameter);
    }

    private void SpawnMeteorVisual()
    {
        if (meteorVisualPrefab == null)
            return;

        meteorVisualInstance =
            Instantiate(
                meteorVisualPrefab,
                impactPoint +
                Vector3.up * meteorHeight,
                Quaternion.identity);
    }

    private void Impact()
    {
        if (warningInstance != null)
            Destroy(warningInstance);

        if (meteorVisualInstance != null)
            Destroy(meteorVisualInstance);

        if (impactVFXPrefab != null)
        {
            GameObject vfx =
                Instantiate(
                    impactVFXPrefab,
                    impactPoint,
                    Quaternion.identity);

            Destroy(vfx, 3f);
        }

        DealDamage();

        Destroy(gameObject);
    }

    private void DealDamage()
    {
        if (damage <= 0f)
            return;

        int count =
            Physics.OverlapSphereNonAlloc(
                impactPoint,
                radius,
                hits,
                playerMask,
                QueryTriggerInteraction.Collide);

        PlayerController damagedPlayer = null;

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i];

            if (col == null)
                continue;

            PlayerController player =
                col.GetComponentInParent<PlayerController>();

            if (player == null ||
                player == damagedPlayer)
            {
                continue;
            }

            damagedPlayer = player;

            player.TakeDamage(damage);

            Rigidbody body =
                player.GetComponent<Rigidbody>();

            if (body != null &&
                (horizontalKnockback > 0f ||
                 verticalKnockback > 0f))
            {
                Vector3 direction =
                    player.transform.position -
                    impactPoint;

                direction.y = 0f;

                if (direction.sqrMagnitude < 0.001f)
                    direction = Vector3.forward;

                direction.Normalize();

                Vector3 force =
                    direction * horizontalKnockback +
                    Vector3.up * verticalKnockback;

                body.AddForce(
                    force,
                    ForceMode.Impulse);
            }

            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radius > 0f ? radius : 2f);
    }

    private void OnDestroy()
    {
        if (warningInstance != null)
            Destroy(warningInstance);

        if (meteorVisualInstance != null)
            Destroy(meteorVisualInstance);
    }
}
