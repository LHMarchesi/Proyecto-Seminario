using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        Vector3 endPoint =
            startPoint +
            Camera.main.transform.forward * attackDistance;

        Collider[] hits = Physics.OverlapCapsule(
            startPoint,
            endPoint,
            radius,
            attackLayer,
            QueryTriggerInteraction.Ignore
        );

        HashSet<BaseEnemy> damagedEnemies =
      new HashSet<BaseEnemy>();

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
   

            if (enemy == null)
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

            damageable.TakeDamage(damage);

            hitSomething = true;

            SpawnHitEffect(
                hitPoint,
                hitNormal
            );

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

            OnMeleeHit?.Invoke(hitInfo);
        }
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