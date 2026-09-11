using System;
using System.Collections;
using UnityEngine;

public class FinalDragonBoss : BaseEnemy
{
    [Header("Fight")]
    [SerializeField] private string bossDisplayName = "Dragon";
    [SerializeField] private GameObject finalDoor;
    [SerializeField] private GameObject deathVFX;
    [SerializeField, Min(0f)] private float deathVFXLifetime = 4f;

    [Header("Fixed positions")]
    [Tooltip("Punto donde queda el dragon durante la Fase 1. Puede quedar null.")]
    [SerializeField] private Transform flightAnchor;
    [Tooltip("Punto central donde combate en tierra. Puede quedar null.")]
    [SerializeField] private Transform groundAnchor;
    [SerializeField, Min(0f)] private float landingDuration = 1.25f;

    [Header("Animations")]
    [SerializeField] private string flightAnimation = "";
    [SerializeField] private string landingAnimation = "";
    [SerializeField] private string idleAnimation = "Idle_Boss";
    [SerializeField] private string clawAnimation = "";
    [SerializeField] private string jumpSlamAnimation = "";
    [SerializeField] private string rangedAttackAnimation = "";
    [SerializeField] private string enrageAnimation = "";

    [Header("Phase 1")]
    [SerializeField, Min(1f)] private float phase1Duration = 22f;

    [Header("Phase 2 -> 3")]
    [SerializeField, Range(0.05f, 0.95f)]
    private float phase3HealthPercent = 0.35f;
    [SerializeField, Min(0f)]
    private float minimumPhase2Duration = 8f;

    [Header("Ground combat")]
    [SerializeField, Min(0.1f)] private float phase2AttackCooldown = 2.2f;
    [SerializeField, Min(0.1f)] private float phase3AttackCooldown = 1.45f;

    [Tooltip("Por encima de esta distancia usa proyectiles. Por debajo usa Claw o Slam.")]
    [SerializeField, Min(0.5f)] private float rangedAttackDistance = 8f;

    [SerializeField, Range(0f, 1f)] private float clawChanceWhenClose = 0.65f;
    [SerializeField, Min(0f)] private float turnSpeed = 5f;

    [Header("Phase 3 speed")]
    [Tooltip("Acelera animaciones, windups, recoveries, giro y proyectiles en Fase 3.")]
    [SerializeField, Min(1f)] private float phase3SpeedMultiplier = 1.30f;

    [Header("Claw")]
    [SerializeField] private Transform clawHitPoint;
    [SerializeField, Min(0.1f)] private float clawRange = 5f;
    [SerializeField, Min(0.1f)] private float clawHitRadius = 2.25f;
    [SerializeField, Min(0f)] private float clawDamage = 18f;
    [SerializeField, Min(0f)] private float clawWindup = 0.55f;
    [SerializeField, Min(0f)] private float clawRecovery = 0.65f;
    [SerializeField, Min(0f)] private float clawHorizontalKnockback = 6f;
    [SerializeField, Min(0f)] private float clawVerticalKnockback = 1.5f;
    [SerializeField] private GameObject clawImpactVFX;
    [SerializeField, Min(0f)] private float clawVFXLifetime = 2f;

    [Header("Jump Slam")]
    [SerializeField, Min(0.1f)] private float slamRadius = 7f;
    [SerializeField, Min(0f)] private float slamDamage = 22f;
    [SerializeField, Min(0f)] private float slamWindup = 0.85f;
    [SerializeField, Min(0f)] private float slamRecovery = 0.9f;
    [SerializeField, Min(0f)] private float slamHorizontalKnockback = 8f;
    [SerializeField, Min(0f)] private float slamVerticalKnockback = 3f;
    [SerializeField] private GameObject slamWarningPrefab;
    [SerializeField] private GameObject slamImpactVFX;
    [SerializeField, Min(0f)] private float slamVFXLifetime = 2.5f;

    [Header("Ranged Projectile")]
    [Tooltip("Prefab visual del proyectil. DragonBossProjectile se agrega automaticamente si falta.")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField, Min(0f)] private float projectileAimHeight = 1.1f;
    [SerializeField, Min(0f)] private float projectileDamage = 14f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 18f;
    [SerializeField, Min(0.05f)] private float projectileHitRadius = 0.55f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
    [SerializeField, Min(0f)] private float projectileWindup = 0.65f;
    [SerializeField, Min(0f)] private float projectileRecovery = 0.55f;
    [SerializeField, Min(0f)] private float projectileHorizontalKnockback = 4f;
    [SerializeField, Min(0f)] private float projectileVerticalKnockback = 1f;
    [SerializeField] private GameObject projectileImpactVFX;
    [SerializeField, Min(0f)] private float projectileImpactVFXLifetime = 2f;

    [Header("Meteor - shared")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask = ~0;
    [SerializeField] private GameObject meteorWarningPrefab;
    [SerializeField] private GameObject meteorVisualPrefab;
    [SerializeField] private GameObject meteorImpactVFX;
    [SerializeField, Min(0.1f)] private float meteorWarningTime = 1.1f;
    [SerializeField, Min(0.1f)] private float meteorRadius = 2.2f;
    [SerializeField, Min(0f)] private float meteorDamage = 16f;
    [SerializeField, Min(0f)] private float meteorHeight = 16f;
    [SerializeField, Min(0f)] private float meteorHorizontalKnockback = 4f;
    [SerializeField, Min(0f)] private float meteorVerticalKnockback = 1f;
    [SerializeField, Min(0f)] private float meteorSpawnGap = 0.12f;
    [SerializeField, Min(0f)] private float meteorRandomRadius = 7f;

    [Header("Meteor - Phase 1")]
    [SerializeField, Min(1)] private int phase1MeteorCount = 4;
    [SerializeField, Min(0.1f)] private float phase1MeteorInterval = 2.4f;

    [Header("Meteor - Phase 2")]
    [Tooltip("0 desactiva los meteoritos durante la Fase 2.")]
    [SerializeField, Min(0f)] private float phase2MeteorInterval = 5f;
    [SerializeField, Min(1)] private int phase2MeteorCount = 1;

    [Header("Meteor - Phase 3")]
    [SerializeField, Min(1)] private int phase3MeteorCount = 3;
    [SerializeField, Min(0.1f)] private float phase3MeteorInterval = 2.8f;

    [Header("Phase 3 feedback")]
    [SerializeField] private GameObject enrageVFX;
    [SerializeField, Min(0f)] private float enrageVFXLifetime = 3f;

    [Header("Diagnostics")]
    [SerializeField] private bool logBoss;

    public event Action<int> OnPhaseChanged;
    public event Action OnBossDefeated;

    public int CurrentPhase => currentPhase;
    public bool FightActive => fightActive;

    private int currentPhase;
    private bool fightActive;
    private bool transitioning;
    private bool isAttacking;
    private bool hasDied;
    private float phase2StartedAt;

    private Coroutine meteorLoop;
    private Coroutine combatLoop;
    private Animator dragonAnimator;

    private readonly Collider[] playerHits = new Collider[32];

    protected override void OnEnable()
    {
        base.OnEnable();

        fightActive = false;
        transitioning = false;
        isAttacking = false;
        hasDied = false;
        currentPhase = 0;

        dragonAnimator = GetComponentInChildren<Animator>();

        if (dragonAnimator != null)
            dragonAnimator.speed = 1f;
    }

    protected override void Update()
    {
        base.Update();

        if (!fightActive || hasDied || target == null)
            return;

        UpdateBossUI();

        if (currentPhase >= 2 && !isAttacking && !transitioning)
            RotateTowardsPlayer();

        if (currentPhase == 2 &&
            Time.time >= phase2StartedAt + minimumPhase2Duration &&
            GetHealthPercent() <= phase3HealthPercent)
        {
            EnterPhase3();
        }
    }

    // El boss se instancia como prefab, por eso los puntos de la arena
    // deben poder venir desde un objeto de escena como DragonBossAltar.
    public void ConfigureArenaAnchors(
        Transform externalFlightAnchor,
        Transform externalGroundAnchor)
    {
        if (externalFlightAnchor != null)
            flightAnchor = externalFlightAnchor;

        if (externalGroundAnchor != null)
            groundAnchor = externalGroundAnchor;
    }

    // El boss NO empieza automáticamente.
    // DragonBossAltar es quien debe llamar este método después de la interacción con E.
    public void BeginBossFight()
    {
        BeginBossFightWithDifficulty(0f);
    }

    // Puede llamarse desde el altar si despues quieren pasar Threat/dificultad.
    public void BeginBossFightWithDifficulty(float difficulty)
    {
        if (fightActive || hasDied)
            return;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        InitializeForWave(difficulty, target);

        if (flightAnchor != null)
        {
            transform.position = flightAnchor.position;
            transform.rotation = flightAnchor.rotation;
        }

        fightActive = true;
        currentPhase = 1;

        SetAnimation(flightAnimation);
        UpdateBossUI();

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 1 iniciada");

        meteorLoop = StartCoroutine(MeteorLoop());
        combatLoop = StartCoroutine(GroundCombatLoop());

        StartCoroutine(Phase1Timer());
    }

    private IEnumerator Phase1Timer()
    {
        yield return new WaitForSeconds(phase1Duration);

        if (!fightActive || hasDied || currentPhase != 1)
            yield break;

        yield return StartCoroutine(TransitionToPhase2());
    }

    private IEnumerator TransitionToPhase2()
    {
        transitioning = true;
        Log("Aterrizando -> Fase 2");

        SetAnimation(landingAnimation);

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition =
            groundAnchor != null
                ? groundAnchor.position
                : transform.position;

        Quaternion endRotation =
            groundAnchor != null
                ? groundAnchor.rotation
                : transform.rotation;

        if (landingDuration > 0f && groundAnchor != null)
        {
            float elapsed = 0f;

            while (elapsed < landingDuration)
            {
                float t = Mathf.Clamp01(elapsed / landingDuration);
                t = t * t * (3f - 2f * t);

                transform.position =
                    Vector3.Lerp(startPosition, endPosition, t);

                transform.rotation =
                    Quaternion.Slerp(startRotation, endRotation, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPosition;
            transform.rotation = endRotation;
        }
        else if (groundAnchor != null)
        {
            transform.position = endPosition;
            transform.rotation = endRotation;
        }

        currentPhase = 2;
        phase2StartedAt = Time.time;
        transitioning = false;

        SetAnimation(idleAnimation);

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 2 iniciada");
    }

    private void EnterPhase3()
    {
        if (currentPhase >= 3 || hasDied)
            return;

        currentPhase = 3;

        if (dragonAnimator == null)
            dragonAnimator = GetComponentInChildren<Animator>();

        if (dragonAnimator != null)
            dragonAnimator.speed = GetCombatSpeedMultiplier();

        if (!isAttacking)
            SetAnimation(enrageAnimation);

        if (enrageVFX != null)
        {
            GameObject vfx = Instantiate(
                enrageVFX,
                CombatVFXPosition,
                Quaternion.identity);

            if (enrageVFXLifetime > 0f)
                Destroy(vfx, enrageVFXLifetime);
        }

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 3 iniciada");
    }

    private IEnumerator GroundCombatLoop()
    {
        while (fightActive && !hasDied)
        {
            if (currentPhase < 2 || transitioning)
            {
                yield return null;
                continue;
            }

            float cooldown =
                currentPhase >= 3
                    ? phase3AttackCooldown
                    : phase2AttackCooldown;

            yield return new WaitForSeconds(cooldown);

            if (!fightActive || hasDied ||
                currentPhase < 2 || transitioning)
            {
                continue;
            }

            yield return StartCoroutine(PerformGroundAttack());
        }
    }

    private IEnumerator PerformGroundAttack()
    {
        if (target == null)
            yield break;

        isAttacking = true;
        FacePlayerImmediate();

        float distance =
            Vector3.Distance(
                transform.position,
                target.position);

        // Lejos: siempre proyectil.
        if (distance > rangedAttackDistance)
        {
            yield return StartCoroutine(
                RangedProjectileAttack());
        }
        // Cerca: Claw o Slam.
        else if (UnityEngine.Random.value <= clawChanceWhenClose)
        {
            yield return StartCoroutine(
                ClawAttack());
        }
        else
        {
            yield return StartCoroutine(
                JumpSlamAttack());
        }

        isAttacking = false;
        SetAnimation(idleAnimation);
    }

    private IEnumerator ClawAttack()
    {
        SetAnimation(clawAnimation);

        if (clawWindup > 0f)
            yield return new WaitForSeconds(
                GetScaledCombatTime(clawWindup));

        Vector3 hitCenter =
            clawHitPoint != null
                ? clawHitPoint.position
                : transform.position +
                  transform.forward * Mathf.Max(1f, clawRange * 0.65f);

        DealDamageToPlayerInSphere(
            hitCenter,
            clawHitRadius,
            clawDamage,
            clawHorizontalKnockback,
            clawVerticalKnockback);

        SpawnVFX(
            clawImpactVFX,
            hitCenter,
            clawVFXLifetime);

        if (clawRecovery > 0f)
            yield return new WaitForSeconds(
                GetScaledCombatTime(clawRecovery));
    }

    private IEnumerator JumpSlamAttack()
    {
        SetAnimation(jumpSlamAnimation);

        GameObject warning =
            SpawnGroundWarning(
                slamWarningPrefab,
                transform.position,
                slamRadius);

        if (slamWindup > 0f)
            yield return new WaitForSeconds(
                GetScaledCombatTime(slamWindup));

        Vector3 impactPoint =
            ProjectToGround(transform.position);

        if (warning != null)
            Destroy(warning);

        SpawnVFX(
            slamImpactVFX,
            impactPoint,
            slamVFXLifetime);

        DealDamageToPlayerInSphere(
            impactPoint,
            slamRadius,
            slamDamage,
            slamHorizontalKnockback,
            slamVerticalKnockback);

        if (slamRecovery > 0f)
            yield return new WaitForSeconds(
                GetScaledCombatTime(slamRecovery));
    }

    private IEnumerator RangedProjectileAttack()
    {
        SetAnimation(rangedAttackAnimation);

        if (projectileWindup > 0f)
        {
            yield return new WaitForSeconds(
                GetScaledCombatTime(projectileWindup));
        }

        // Re-apunta justo antes de disparar.
        FacePlayerImmediate();
        SpawnProjectile();

        if (projectileRecovery > 0f)
        {
            yield return new WaitForSeconds(
                GetScaledCombatTime(projectileRecovery));
        }
    }

    private void SpawnProjectile()
    {
        if (target == null)
            return;

        if (projectilePrefab == null)
        {
            Log("Projectile Prefab no asignado.");
            return;
        }

        Vector3 spawnPosition =
            projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position +
                  transform.forward * 2.5f +
                  Vector3.up * 1.5f;

        Vector3 aimPosition =
            target.position +
            Vector3.up * projectileAimHeight;

        Vector3 direction =
            aimPosition - spawnPosition;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.forward;

        direction.Normalize();

        GameObject projectileObject =
            Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(direction));

        DragonBossProjectile projectile =
            projectileObject.GetComponent<DragonBossProjectile>();

        if (projectile == null)
            projectile =
                projectileObject.AddComponent<DragonBossProjectile>();

        projectile.Initialize(
            direction,
            projectileSpeed * GetCombatSpeedMultiplier(),
            projectileDamage,
            projectileHitRadius,
            projectileLifetime,
            playerMask,
            projectileHorizontalKnockback,
            projectileVerticalKnockback,
            projectileImpactVFX,
            projectileImpactVFXLifetime);
    }

    private float GetCombatSpeedMultiplier()
    {
        return currentPhase >= 3
            ? Mathf.Max(1f, phase3SpeedMultiplier)
            : 1f;
    }

    private float GetScaledCombatTime(float baseTime)
    {
        return baseTime /
               GetCombatSpeedMultiplier();
    }

    private IEnumerator MeteorLoop()
    {
        while (fightActive && !hasDied)
        {
            if (transitioning || target == null)
            {
                yield return null;
                continue;
            }

            int count = GetMeteorCount();
            float interval = GetMeteorInterval();

            if (count <= 0 || interval <= 0f)
            {
                yield return null;
                continue;
            }

            yield return StartCoroutine(SpawnMeteorVolley(count));

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator SpawnMeteorVolley(int count)
    {
        if (target == null)
            yield break;

        Vector3 playerPosition = target.position;

        for (int i = 0; i < count; i++)
        {
            Vector3 candidate;

            // El primer meteorito siempre obliga al jugador a moverse.
            if (i == 0)
            {
                candidate = playerPosition;
            }
            else
            {
                Vector2 random =
                    UnityEngine.Random.insideUnitCircle *
                    meteorRandomRadius;

                candidate =
                    playerPosition +
                    new Vector3(random.x, 0f, random.y);
            }

            Vector3 groundPoint =
                ProjectToGround(candidate);

            CreateMeteorHazard(groundPoint);

            if (meteorSpawnGap > 0f && i < count - 1)
                yield return new WaitForSeconds(meteorSpawnGap);
        }
    }

    private void CreateMeteorHazard(Vector3 groundPoint)
    {
        GameObject hazardObject =
            new GameObject("Dragon_Meteor_Hazard");

        hazardObject.transform.position = groundPoint;

        DragonMeteorHazard hazard =
            hazardObject.AddComponent<DragonMeteorHazard>();

        hazard.Initialize(
            groundPoint,
            playerMask,
            meteorWarningPrefab,
            meteorVisualPrefab,
            meteorImpactVFX,
            meteorWarningTime,
            meteorRadius,
            meteorDamage,
            meteorHeight,
            meteorHorizontalKnockback,
            meteorVerticalKnockback);
    }

    private int GetMeteorCount()
    {
        if (currentPhase == 1)
            return phase1MeteorCount;

        if (currentPhase == 2)
            return phase2MeteorInterval > 0f
                ? phase2MeteorCount
                : 0;

        if (currentPhase >= 3)
            return phase3MeteorCount;

        return 0;
    }

    private float GetMeteorInterval()
    {
        if (currentPhase == 1)
            return phase1MeteorInterval;

        if (currentPhase == 2)
            return phase2MeteorInterval;

        if (currentPhase >= 3)
            return phase3MeteorInterval;

        return 0f;
    }

    private void DealDamageToPlayerInSphere(
        Vector3 center,
        float radius,
        float damage,
        float horizontalKnockback,
        float verticalKnockback)
    {
        int hitCount =
            Physics.OverlapSphereNonAlloc(
                center,
                radius,
                playerHits,
                playerMask,
                QueryTriggerInteraction.Collide);

        PlayerController damagedPlayer = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = playerHits[i];

            if (col == null)
                continue;

            PlayerController player =
                col.GetComponentInParent<PlayerController>();

            if (player == null || player == damagedPlayer)
                continue;

            damagedPlayer = player;
            player.TakeDamage(damage);

            Rigidbody playerBody =
                player.GetComponent<Rigidbody>();

            if (playerBody != null &&
                (horizontalKnockback > 0f ||
                 verticalKnockback > 0f))
            {
                Vector3 direction =
                    player.transform.position - center;

                direction.y = 0f;

                if (direction.sqrMagnitude < 0.001f)
                    direction = transform.forward;

                direction.Normalize();

                Vector3 force =
                    direction * horizontalKnockback +
                    Vector3.up * verticalKnockback;

                playerBody.AddForce(
                    force,
                    ForceMode.Impulse);
            }

            break;
        }
    }

    private Vector3 ProjectToGround(Vector3 position)
    {
        int mask =
            groundMask.value != 0
                ? groundMask.value
                : LayerMask.GetMask("Ground");

        Vector3 rayStart =
            position + Vector3.up * 25f;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            60f,
            mask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return new Vector3(
            position.x,
            transform.position.y,
            position.z);
    }

    private GameObject SpawnGroundWarning(
        GameObject prefab,
        Vector3 position,
        float radius)
    {
        if (prefab == null)
            return null;

        Vector3 groundPoint =
            ProjectToGround(position);

        GameObject warning =
            Instantiate(
                prefab,
                groundPoint + Vector3.up * 0.02f,
                Quaternion.identity);

        Vector3 scale = warning.transform.localScale;
        float diameter = radius * 2f;

        warning.transform.localScale =
            new Vector3(
                diameter,
                scale.y,
                diameter);

        return warning;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed *
                GetCombatSpeedMultiplier() *
                Time.deltaTime);
    }

    private void FacePlayerImmediate()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation =
                Quaternion.LookRotation(direction.normalized);
    }

    private void SetAnimation(string animationName)
    {
        if (handleAnimations == null ||
            string.IsNullOrWhiteSpace(animationName))
        {
            return;
        }

        handleAnimations.ChangeAnimationState(
            animationName,
            true);
    }

    private void SpawnVFX(
        GameObject prefab,
        Vector3 position,
        float lifetime)
    {
        if (prefab == null)
            return;

        GameObject vfx =
            Instantiate(
                prefab,
                position,
                Quaternion.identity);

        if (lifetime > 0f)
            Destroy(vfx, lifetime);
    }

    private float GetHealthPercent()
    {
        float max =
            CurrentStats != null
                ? CurrentStats.maxHealth
                : maxHealth;

        if (max <= 0f)
            return 0f;

        return Mathf.Clamp01(currentHealth / max);
    }

    protected override void OnDamage(
        float damage,
        DamageFeedbackType feedbackType)
    {
        if (hasDied)
            return;

        base.OnDamage(damage, feedbackType);

        ShowDamageNumber(
            damage,
            feedbackType);

        if (!IsDead())
            UpdateBossUI();
    }

    protected override void Die(float experienceDroped = 0f)
    {
        if (hasDied)
            return;

        hasDied = true;
        fightActive = false;
        isAttacking = false;

        if (dragonAnimator != null)
            dragonAnimator.speed = 1f;

        StopAllCoroutines();

        if (finalDoor != null)
            finalDoor.SetActive(true);

        if (deathVFX != null)
        {
            GameObject vfx =
                Instantiate(
                    deathVFX,
                    CombatVFXPosition,
                    Quaternion.identity);

            if (deathVFXLifetime > 0f)
                Destroy(vfx, deathVFXLifetime);
        }

        if (UIManager.Instance != null)
            UIManager.Instance.DisableBossName();

        OnBossDefeated?.Invoke();

        // Importante para Poison/Fire/otros efectos que escuchan OnDeath.
        OnDeath?.Invoke();

        gameObject.SetActive(false);
    }

    private void UpdateBossUI()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.SetBossName(
            bossDisplayName);

        UIManager.Instance.SetBossHealth(
            currentHealth);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 clawCenter =
            clawHitPoint != null
                ? clawHitPoint.position
                : transform.position +
                  transform.forward * Mathf.Max(1f, clawRange * 0.65f);

        Gizmos.DrawWireSphere(
            clawCenter,
            clawHitRadius);

        Gizmos.DrawWireSphere(
            transform.position,
            slamRadius);
    }

    private void Log(string message)
    {
        if (logBoss)
            Debug.Log(
                "[FinalDragonBoss] " + message,
                this);
    }
}
