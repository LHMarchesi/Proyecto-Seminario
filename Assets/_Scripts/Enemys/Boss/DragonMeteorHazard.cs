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

    private AudioClip fallSound;
    private AudioClip impactSound;
    private float fallVolume;
    private float impactVolume;
    private float audioSpatialBlend;
    private float audioMinDistance;
    private float audioMaxDistance;

    private AudioSource fallAudioSource;

    private Vector3 impactPoint;
    private GameObject warningInstance;
    private GameObject meteorVisualInstance;

    private readonly Collider[] hits =
        new Collider[24];

    // Overload legacy para no romper llamadas antiguas.
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
        Initialize(
            point,
            players,
            warning,
            meteorVisual,
            impactVFX,
            telegraphTime,
            impactRadius,
            impactDamage,
            visualHeight,
            horizontalForce,
            verticalForce,
            null,
            null,
            0f,
            0f,
            1f,
            4f,
            45f);
    }

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
        float verticalForce,
        AudioClip fallingClip,
        AudioClip impactClip,
        float fallingVolume,
        float hitVolume,
        float spatialBlend,
        float minimumDistance,
        float maximumDistance)
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

        fallSound = fallingClip;
        impactSound = impactClip;
        fallVolume = Mathf.Clamp01(fallingVolume);
        impactVolume = Mathf.Clamp01(hitVolume);
        audioSpatialBlend = Mathf.Clamp01(spatialBlend);
        audioMinDistance = Mathf.Max(0.1f, minimumDistance);
        audioMaxDistance = Mathf.Max(audioMinDistance, maximumDistance);

        transform.position = impactPoint;

        StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        SpawnWarning();
        SpawnMeteorVisual();
        StartFallSound();

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

        PlayImpactSound();
        DealDamage();

        Destroy(gameObject);
    }

    private void StartFallSound()
    {
        if (fallSound == null ||
            fallVolume <= 0f)
        {
            return;
        }

        fallAudioSource =
            gameObject.AddComponent<AudioSource>();

        fallAudioSource.playOnAwake = false;
        fallAudioSource.clip = fallSound;
        fallAudioSource.volume = fallVolume;
        fallAudioSource.spatialBlend = audioSpatialBlend;
        fallAudioSource.minDistance = audioMinDistance;
        fallAudioSource.maxDistance = audioMaxDistance;
        fallAudioSource.rolloffMode = AudioRolloffMode.Linear;

        // El AudioSource vive en el hazard, que esta colocado en el punto
        // de impacto. El clip funciona como aviso espacial del meteorito.
        fallAudioSource.Play();
    }

    private void PlayImpactSound()
    {
        if (impactSound == null ||
            impactVolume <= 0f)
        {
            return;
        }

        GameObject audioObject =
            new GameObject(
                "DragonMeteor_ImpactAudio");

        audioObject.transform.position =
            impactPoint;

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.clip = impactSound;
        source.volume = impactVolume;
        source.spatialBlend = audioSpatialBlend;
        source.minDistance = audioMinDistance;
        source.maxDistance = audioMaxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();

        Destroy(
            audioObject,
            impactSound.length + 0.15f);
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
