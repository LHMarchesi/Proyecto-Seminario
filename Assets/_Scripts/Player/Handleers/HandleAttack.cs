using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed class MeleeAttackContext
{
    public MeleeAttackType AttackType { get; private set; }
    public float Damage { get; set; }
    public float Radius { get; set; }
    public float KnockbackForce { get; set; }
    public Vector3 Origin { get; private set; }
    public Vector3 Forward { get; private set; }

    public MeleeAttackContext(MeleeAttackType attackType, float damage, float radius,
        float knockbackForce, Vector3 origin, Vector3 forward)
    {
        AttackType = attackType;
        Damage = damage;
        Radius = radius;
        KnockbackForce = knockbackForce;
        Origin = origin;
        Forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }
}

public class HandleAttack : MonoBehaviour
{
    private PlayerContext playerContext;

    [Header("Attacking")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackSpeed;
    [SerializeField] public int attackDamage;
    [SerializeField] private LayerMask attackLayer;

    [Header("Feedback")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;

    // Los Build Items escuchan este evento.
    public event Action<MeleeHitInfo> OnMeleeHit;
    // Se dispara antes del daño base. Útil para aplicar statuses al golpe letal.
    public event Action<MeleeHitInfo> OnMeleeHitBeforeDamage;
    public event Action<IReadOnlyList<MeleeHitInfo>> OnMeleeAttackResolved;
    // Permite modificar el daño/radio/fuerza del ataque antes de consultar enemigos.
    public event Action<MeleeAttackContext> OnMeleeAttackPreparing;
    // Se emite una vez por ataque ejecutado, incluso si no golpeó a nadie.
    public event Action<MeleeAttackContext> OnMeleeAttackExecuted;
    // Se dispara una sola vez por aterrizaje, desde FallingWithHammer.
    [Header("Ground impact")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundRayDistance = 3f;
    [SerializeField] private float fallingBaseRadius = 20f;
    [SerializeField] private float fallingBaseDamage = 100f;
    public LayerMask EnemyHitLayer => attackLayer;
    public float FallingBaseRadius => fallingBaseRadius;
    public float FallingBaseDamage => fallingBaseDamage;

    public Vector3 GetGroundImpactPoint(Vector3 playerPosition)
    {
        Vector3 origin = playerPosition + Vector3.up * 0.3f;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down,
            groundRayDistance, groundLayer, QueryTriggerInteraction.Ignore);
        float closest = float.PositiveInfinity;
        Vector3 result = playerPosition;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform) ||
                hit.collider.GetComponentInParent<BaseEnemy>() != null ||
                hit.collider.GetComponentInParent<Mjolnir>() != null) continue;
            if (hit.distance < closest)
            {
                closest = hit.distance;
                result = hit.point;
            }
        }
        return result;
    }

    public event Action<Vector3> OnFallingHammerLanded;

    public void NotifyFallingHammerLanding(Vector3 groundPoint)
    {
        OnFallingHammerLanded?.Invoke(groundPoint);
    }

    private bool attacking = false;
    private bool readyToAttack = true;

    private float playerSpeed;

    private Coroutine hitStopRoutine;

    private void Awake()
    {
        playerContext = GetComponent<PlayerContext>();
    }

    /// <summary>
    /// Ejecuta un ataque melee.
    ///
    /// knockbackForce es opcional para no obligarte a modificar
    /// inmediatamente todos tus AttackStates.
    ///
    /// Cuando conectemos el knockback propio de cada ataque,
    /// simplemente se lo pasamos acá.
    /// </summary>
    public void Attack(
        float damage,
        float radius,
        float shakeDuration,
        float shakeMagnitude,
        float kickPitch,
        float kickYaw,
        float hitStopDuration,
        MeleeAttackType attackType,
        float knockbackForce = 0f)
    {
        if (!readyToAttack || attacking)
            return;

        StartCoroutine(
            DoAttack(
                damage,
                radius,
                shakeDuration,
                shakeMagnitude,
                kickPitch,
                kickYaw,
                hitStopDuration,
                attackType,
                knockbackForce
            )
        );
    }

    public void PlayHitAttackSound()
    {
        string[] attackSounds =
        {
            "AttackHit2",
            "AttackHit3"
        };

        int index = UnityEngine.Random.Range(
            0,
            attackSounds.Length
        );

        SoundManagerOcta.Instance.PlaySound(
            attackSounds[index]
        );
    }

    public void PlayAttackSound()
    {
        string[] attackSounds =
        {
            "Attack2",
            "Attack3"
        };

        int index = UnityEngine.Random.Range(
            0,
            attackSounds.Length
        );

        SoundManagerOcta.Instance.PlaySound(
            attackSounds[index]
        );
    }

    private IEnumerator DoAttack(
        float damage,
        float radius,
        float shakeDuration,
        float shakeMagnitude,
        float kickPitch,
        float kickYaw,
        float hitStopDuration,
        MeleeAttackType attackType,
        float knockbackForce)
    {
        readyToAttack = false;
        attacking = true;

        bool hitSomething = false;

        // Evita pegar varias veces al mismo IDamageable
        // si el enemigo tiene más de un collider.
        HashSet<IDamageable> damagedTargets =
            new HashSet<IDamageable>();

        PlayAttackSound();

        // Guardamos la velocidad previa.
        playerSpeed =
            playerContext.PlayerController.currentSpeed;

        playerContext.PlayerController.ChangeSpeed(
            playerContext.PlayerController.currentSpeed -
            playerContext.PlayerController.playerStats.speedReductor
        );

        // Esperamos hasta el momento real del impacto.
        yield return new WaitForSeconds(attackDelay);

        Vector3 startPoint =
     Camera.main.transform.position;

        Vector3 attackForward = Camera.main.transform.forward;
        MeleeAttackContext attackContext = new MeleeAttackContext(
            attackType, damage, radius, knockbackForce, startPoint, attackForward);
        OnMeleeAttackPreparing?.Invoke(attackContext);
        damage = Mathf.Max(0f, attackContext.Damage);
        radius = Mathf.Max(0f, attackContext.Radius);
        knockbackForce = Mathf.Max(0f, attackContext.KnockbackForce);
        // Los mismos valores finales llegan a los eventos y al daño base.
        attackContext.Damage = damage;
        attackContext.Radius = radius;
        attackContext.KnockbackForce = knockbackForce;

        Vector3 endPoint =
            startPoint +
            attackForward * attackDistance;

        Collider[] hits = Physics.OverlapCapsule(
            startPoint,
            endPoint,
            radius,
            attackLayer,
            QueryTriggerInteraction.Ignore
        );

        List<MeleeHitInfo> resolvedHits = new List<MeleeHitInfo>();
        HashSet<BaseEnemy> damagedEnemies =
      new HashSet<BaseEnemy>();

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();


            if (enemy == null || enemy.IsDead())
                continue;

            if (!damagedEnemies.Add(enemy))
                continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();


            if (damageable == null)
                continue;

            Vector3 hitPoint;
            Vector3 hitNormal;

            GetHitSurface(
                hit,
                out hitPoint,
                out hitNormal
            );

            Vector3 hitDirection =
                (
                    enemy.transform.position -
                    transform.position
                ).normalized;

            // Snapshot antes del daño: un golpe letal puede desactivar el enemigo.
            EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
            bool wasBurning = status != null && status.IsBurning;
            Vector3 targetPosition = enemy.transform.position;
            Vector3 visualPosition = enemy.CombatVFXPosition;

            MeleeHitInfo hitInfo =
                new MeleeHitInfo(
                    attackType,
                    hit,
                    enemy,
                    hitPoint,
                    hitDirection,
                    damage,
                    knockbackForce
                );

            hitInfo.WasBurning = wasBurning;
            hitInfo.TargetPosition = targetPosition;
            hitInfo.VisualPosition = visualPosition;

            // Muspel aplica Burn antes de que un golpe letal desactive al enemigo.
            // No hace daño inmediato: sólo inicia/refresca el status.
            OnMeleeHitBeforeDamage?.Invoke(hitInfo);

            damageable.TakeDamage(damage);

            hitSomething = true;

            SpawnHitEffect(
                hitPoint,
                hitNormal
            );

            // Se conservan los eventos existentes y su orden posterior al daño.
            OnMeleeHit?.Invoke(hitInfo);
            resolvedHits.Add(hitInfo);
        }

        if (resolvedHits.Count > 0)
        {
            OnMeleeAttackResolved?.Invoke(resolvedHits);
        }

        OnMeleeAttackExecuted?.Invoke(attackContext);

        // FEEDBACK  DEL GOLPE

        if (hitSomething)
        {
            PlayHitAttackSound();

            CameraManager.Instance.DoScreenShake(
                shakeDuration,
                shakeMagnitude
            );

            CameraManager.Instance.DoCameraKick(
                kickPitch,
                UnityEngine.Random.Range(
                    -kickYaw,
                    kickYaw
                )
            );

            yield return HitStopRoutine(
                hitStopDuration
            );
        }

        playerContext.PlayerController.ChangeSpeed(
            playerSpeed
        );

        attacking = false;
        readyToAttack = true;
    }

    private void SpawnHitEffect(
    Vector3 position,
    Vector3 normal)
    {
        if (hitEffect == null)
            return;

        // Evita que el VFX quede metido dentro del modelo.
        Vector3 spawnPosition =
            position + normal * 0.08f;

        Quaternion rotation =
            Quaternion.LookRotation(normal);

        GameObject effect = Instantiate(
            hitEffect,
            spawnPosition,
            rotation
        );

        Destroy(effect, 3f);
    }

    private bool GetHitSurface(
    Collider targetCollider,
    out Vector3 hitPoint,
    out Vector3 hitNormal)
    {
        Vector3 cameraPosition =
            Camera.main.transform.position;

        Vector3 targetPosition =
            targetCollider.bounds.center;

        Vector3 direction =
            (targetPosition - cameraPosition).normalized;

        Ray ray = new Ray(
            cameraPosition,
            direction
        );

        if (targetCollider.Raycast(
            ray,
            out RaycastHit rayHit,
            attackDistance + 3f))
        {
            hitPoint = rayHit.point;
            hitNormal = rayHit.normal;

            return true;
        }

        // Fallback por si el collider no acepta correctamente el Raycast.
        Vector3 pointOutside =
            targetCollider.bounds.center -
            direction * 5f;

        hitPoint =
            targetCollider.ClosestPoint(
                pointOutside
            );

        hitNormal =
            (hitPoint - targetCollider.bounds.center)
            .normalized;

        return false;
    }

    private IEnumerator HitStopRoutine(
        float duration)
    {
        if (duration <= 0f)
            yield break;

        float previousTimeScale =
            Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(
            duration
        );

        Time.timeScale =
            previousTimeScale;

        hitStopRoutine = null;
    }
}