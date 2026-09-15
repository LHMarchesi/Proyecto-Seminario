using System.Collections.Generic;
using UnityEngine;

public class ValkyriesDescentPowerUp : BasePowerUp
{
    [Header("Shockwave")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float damage = 20f;

    [Tooltip("Lv2: aumenta uniformemente el tamaño del VFX y el radio efectivo.")]
    [SerializeField] private float level2ScaleMultiplier = 1.25f;

    [SerializeField] private float level3DamageMultiplier = 1.25f;
    [SerializeField] private float level4UpwardForce = 6f;

    [Header("Shockwave VFX")]
    [SerializeField] private GameObject shockwaveVFXPrefab;

    [Tooltip("Offset vertical del VFX respecto al punto de impacto.")]
    [SerializeField] private float shockwaveVFXHeightOffset = 0f;

    [SerializeField] private float vfxLifetime = 2f;

    [Header("Electricity - From Level 1")]
    [Tooltip("La shockwave aplica Electricity desde Lv1. El stun y DPS salen de este data.")]
    [SerializeField] private ElectricityApplicationData electricityData;

    [Header("Level 5 - Lightning")]
    [SerializeField] private GameObject lightningVFXPrefab;
    [SerializeField] private Vector3 lightningVFXOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float lightningDamage = 12f;

    private int level = 1;
    private bool subscribed;

    protected override void ApplyEffect()
    {
        if (subscribed ||
            playerContext == null ||
            playerContext.HandleAttack == null)
        {
            return;
        }

        playerContext.HandleAttack.OnFallingHammerLanded += HandleLanding;
        subscribed = true;
    }

    private void HandleLanding(Vector3 groundPoint)
    {
        float scaleMultiplier =
            level >= 2
                ? level2ScaleMultiplier
                : 1f;

        float finalRadius =
            radius * scaleMultiplier;

        float finalDamage =
            damage *
            (level >= 3
                ? level3DamageMultiplier
                : 1f);

        SpawnShockwaveVFX(
            groundPoint,
            scaleMultiplier);

        CombatAreaDamage.DealDamage(
            groundPoint,
            finalRadius,
            enemyLayer,
            finalDamage,
            DamageFeedbackType.Normal);

        List<BaseEnemy> targets =
            CombatAreaDamage.FindTargets(
                groundPoint,
                finalRadius,
                enemyLayer);

        // Electricity se aplica desde nivel 1.
        // El propio ElectricityApplicationData define stun, DPS,
        // duración y tick interval.
        foreach (BaseEnemy enemy in targets)
        {
            if (enemy == null ||
                enemy.IsDead())
            {
                continue;
            }

            EnemyStatusEffectController status =
                enemy.GetComponent<EnemyStatusEffectController>();

            if (status != null)
            {
                status.ApplyElectricity(
                    electricityData);
            }
        }

        // Lv4: impulso vertical.
        if (level >= 4 &&
            level4UpwardForce > 0f)
        {
            foreach (BaseEnemy enemy in targets)
            {
                if (enemy == null ||
                    enemy.IsDead())
                {
                    continue;
                }

                Rigidbody body =
                    enemy.GetComponent<Rigidbody>();

                if (body != null &&
                    !body.isKinematic)
                {
                    body.AddForce(
                        Vector3.up *
                        level4UpwardForce,
                        ForceMode.Impulse);
                }
            }
        }

        if (level < 5)
            return;

        SpawnVFX(
            lightningVFXPrefab,
            groundPoint +
            lightningVFXOffset);

        // Lv5 mantiene el golpe eléctrico adicional.
        // Electricity ya fue aplicada por la shockwave anteriormente.
        foreach (BaseEnemy enemy in targets)
        {
            if (enemy == null ||
                enemy.IsDead())
            {
                continue;
            }

            if (lightningDamage > 0f)
            {
                enemy.TakeEffectDamage(
                    lightningDamage,
                    DamageFeedbackType.Electricity);
            }
        }
    }

    private void SpawnShockwaveVFX(
        Vector3 groundPoint,
        float scaleMultiplier)
    {
        if (shockwaveVFXPrefab == null)
            return;

        Vector3 forward =
            GetPlayerForward();

        Vector3 spawnPosition =
            groundPoint +
            Vector3.up *
            shockwaveVFXHeightOffset;

        Quaternion spawnRotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up);

        GameObject vfx =
            Instantiate(
                shockwaveVFXPrefab,
                spawnPosition,
                spawnRotation);

        // Lv2 escala el efecto completo uniformemente.
        if (scaleMultiplier > 0f)
        {
            vfx.transform.localScale *=
                scaleMultiplier;
        }

        if (vfxLifetime > 0f)
        {
            Destroy(
                vfx,
                vfxLifetime);
        }
    }

    private Vector3 GetPlayerForward()
    {
        Vector3 forward =
            Vector3.forward;

        if (Camera.main != null)
        {
            forward =
                Camera.main.transform.forward;
        }
        else if (playerContext != null &&
                 playerContext.PlayerController != null)
        {
            forward =
                playerContext.PlayerController
                    .transform.forward;
        }

        // Mantiene la shockwave horizontal.
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f &&
            playerContext != null &&
            playerContext.PlayerController != null)
        {
            forward =
                playerContext.PlayerController
                    .transform.forward;

            forward.y = 0f;
        }

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return forward.normalized;
    }

    private void SpawnVFX(
        GameObject prefab,
        Vector3 position)
    {
        if (prefab == null)
            return;

        GameObject vfx =
            Instantiate(
                prefab,
                position,
                Quaternion.identity);

        if (vfxLifetime > 0f)
        {
            Destroy(
                vfx,
                vfxLifetime);
        }
    }

    protected override void Upgrade()
    {
        level =
            Mathf.Min(
                5,
                level + 1);
    }

    private void OnDestroy()
    {
        if (subscribed &&
            playerContext != null &&
            playerContext.HandleAttack != null)
        {
            playerContext.HandleAttack.OnFallingHammerLanded -= HandleLanding;
        }

        subscribed = false;
    }
}
