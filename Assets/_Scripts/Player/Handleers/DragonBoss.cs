using System;
using System.Collections;
using UnityEngine;

public class DragonBoss : BaseEnemy
{
    [Header("Fight")]
    [SerializeField] private string bossDisplayName = "Dragon";
    [SerializeField] private GameObject finalDoor;
    [SerializeField] private GameObject deathVFX;
    [SerializeField, Min(0f)] private float deathVFXLifetime = 4f;

    [Header("Death Feedback")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] private float deathSoundVolume = 1f;

    [Tooltip("0 = global/2D. Recomendado para que la muerte del boss siempre se escuche.")]
    [SerializeField, Range(0f, 1f)] private float deathSoundSpatialBlend = 0f;

    [Tooltip("Tiempo antes de ocultar completamente al dragon despues de morir.")]
    [SerializeField, Min(0f)] private float deathDisappearDelay = 0.15f;

    [Header("Fixed positions")]
    [Tooltip("Fallback legacy para Fase 1 si no se configuraron Flight Points.")]
    [SerializeField] private Transform flightAnchor;

    [Tooltip("Puntos que el dragon recorre durante la Fase 1. Normalmente los envia DragonBossAltar.")]
    [SerializeField] private Transform[] flightPoints;

    [Tooltip("Punto central donde combate en tierra. Puede quedar null.")]
    [SerializeField] private Transform groundAnchor;

    [SerializeField, Min(0f)] private float landingDuration = 1.25f;

    [Header("Boss Intro")]
    [Tooltip("Animacion opcional para el rugido inicial. Si queda vacio usa Flight.")]
    [SerializeField] private string introRoarAnimation = "";

    [SerializeField] private AudioClip introRoarSound;
    [SerializeField, Range(0f, 1f)] private float introRoarVolume = 1f;

    [Tooltip("0 = rugido global/2D. Recomendado para la entrada del boss.")]
    [SerializeField, Range(0f, 1f)] private float introRoarSpatialBlend = 0f;

    [Tooltip("Tiempo breve antes de comenzar el bombardeo y el vuelo continuo.")]
    [SerializeField, Min(0f)] private float introDuration = 0.9f;

    [Tooltip("Delay desde que empieza el rugido hasta que se dispara el screen shake.")]
    [SerializeField, Min(0f)] private float introShakeDelay = 0f;

    [SerializeField, Min(0f)] private float introShakeDuration = 0.7f;
    [SerializeField, Min(0f)] private float introShakeMagnitude = 0.18f;

    [Header("Boss Audio")]
    [SerializeField] private AudioClip rangedAttackSound;
    [SerializeField, Range(0f, 1f)] private float rangedAttackVolume = 0.9f;

    [Tooltip("Se elige uno al azar cada vez que el boss recibe dano, respetando el cooldown.")]
    [SerializeField] private AudioClip[] damageSounds;
    [SerializeField, Range(0f, 1f)] private float damageSoundVolume = 0.85f;
    [SerializeField, Min(0f)] private float damageSoundCooldown = 0.18f;
    [SerializeField, Range(0.1f, 3f)] private float damagePitchMin = 0.92f;
    [SerializeField, Range(0.1f, 3f)] private float damagePitchMax = 1.08f;

    [Tooltip("0 = 2D, 1 = completamente 3D.")]
    [SerializeField, Range(0f, 1f)] private float bossAudioSpatialBlend = 1f;
    [SerializeField, Min(0.1f)] private float bossAudioMinDistance = 4f;
    [SerializeField, Min(0.1f)] private float bossAudioMaxDistance = 45f;

    [Header("Phase 1 - Flight movement")]
    [Tooltip("Velocidad constante del dragon mientras recorre los Flight Points.")]
    [SerializeField, Min(0.1f)] private float phase1FlightSpeed = 9f;

    [Tooltip("Velocidad con la que el dragon gira hacia la direccion de vuelo.")]
    [SerializeField, Min(0.1f)] private float phase1FlightTurnSpeed = 4f;

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

    [Header("Phase 3 - Air Assault")]
    [Tooltip("Tiempo que permanece en el Flight Anchor usando proyectiles + meteoritos.")]
    [SerializeField, Min(1f)] private float phase3AirDuration = 27.5f;

    [Tooltip("Tiempo que tarda en subir desde Ground Anchor hasta Flight Anchor.")]
    [SerializeField, Min(0f)] private float phase3TakeoffDuration = 1.5f;

    [Tooltip("Tiempo que tarda en volver al Ground Anchor para comenzar Fase 4.")]
    [SerializeField, Min(0f)] private float phase3ReturnDuration = 1.5f;

    [Header("Ground combat")]
    [SerializeField, Min(0.1f)] private float phase2AttackCooldown = 2.2f;

    [Tooltip("Cooldown usado por la Fase 3 aerea y la Fase 4 final.")]
    [SerializeField, Min(0.1f)] private float phase3AttackCooldown = 1.45f;

    [Tooltip("Por encima de esta distancia usa proyectiles. Por debajo usa Claw o Slam.")]
    [SerializeField, Min(0.5f)] private float rangedAttackDistance = 8f;

    [SerializeField, Range(0f, 1f)] private float clawChanceWhenClose = 0.65f;
    [SerializeField, Min(0f)] private float turnSpeed = 5f;

    [Header("Phase 3 / 4 speed")]
    [Tooltip("Acelera animaciones, windups, recoveries, giro y proyectiles en Fases 3 y 4.")]
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

    [Header("Meteor - Audio")]
    [Tooltip("Whoosh/caida que empieza junto al telegraph.")]
    [SerializeField] private AudioClip meteorFallSound;
    [SerializeField, Range(0f, 1f)] private float meteorFallVolume = 0.75f;

    [Tooltip("Impacto grave que se reproduce cuando el meteorito toca el suelo.")]
    [SerializeField] private AudioClip meteorImpactSound;
    [SerializeField, Range(0f, 1f)] private float meteorImpactVolume = 0.95f;

    [Header("Meteor - Phase 1 - Continuous stream")]
    [Tooltip("Tiempo entre cada meteorito durante toda la Fase 1. No depende de los Flight Points.")]
    [SerializeField, Min(0.1f)] private float phase1MeteorStreamInterval = 0.75f;

    [Tooltip("Probabilidad de que un meteorito apunte exactamente a la posicion actual del jugador. El resto cae cerca.")]
    [SerializeField, Range(0f, 1f)] private float phase1TargetedMeteorChance = 0.55f;

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

    [Tooltip("Sonido que marca el comienzo de la Fase 3 aerea.")]
    [SerializeField] private AudioClip phase3TransitionSound;
    [SerializeField, Range(0f, 1f)] private float phase3TransitionVolume = 1f;

    [Tooltip("0 = global/2D. Recomendado para comunicar claramente el cambio de fase.")]
    [SerializeField, Range(0f, 1f)] private float phase3TransitionSpatialBlend = 0f;

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
    private Coroutine phase1FlightLoop;
    private Coroutine phase3Routine;
    private Animator dragonAnimator;

    private float nextDamageSoundTime;
    private int lastDamageSoundIndex = -1;

    // Seguridad del loop de combate.
    // Si por cualquier razón una coroutine de ataque queda interrumpida,
    // evitamos que isAttacking bloquee el boss para siempre.
    private float lastCombatProgressTime;

    private readonly Collider[] playerHits = new Collider[32];

    protected override void OnEnable()
    {
        base.OnEnable();

        fightActive = false;
        transitioning = false;
        isAttacking = false;
        hasDied = false;
        currentPhase = 0;
        nextDamageSoundTime = 0f;
        lastDamageSoundIndex = -1;
        lastCombatProgressTime = Time.time;

        dragonAnimator = GetComponentInChildren<Animator>();

        // IMPORTANTE:
        // EnemyStatusEffectController normalmente implementa el stun
        // deshabilitando el BaseEnemy. En este boss eso deshabilitaria
        // DragonBoss y rompería el estado de fases al reactivarse.
        // El boss sigue recibiendo Electricity, VFX y DoT, pero no
        // el hard-stun que apaga su componente.
        EnemyStatusEffectController statusEffects =
            GetComponent<EnemyStatusEffectController>();

        if (statusEffects != null)
            statusEffects.SetElectricityStunAllowed(false);

        if (dragonAnimator != null)
            dragonAnimator.speed = 1f;
    }

    protected override void Update()
    {
        base.Update();

        if (!fightActive || hasDied || target == null)
            return;

        UpdateBossUI();

        if (currentPhase >= 2 &&
            !transitioning)
        {
            RecoverCombatLoopIfStalled();
        }

        if (currentPhase >= 2 &&
            !isAttacking &&
            !transitioning)
        {
            RotateTowardsPlayer();
        }

        // La transición de fase NO depende de isAttacking.
        // Si un ataque quedó trabado, la fase igualmente puede avanzar.
        if (currentPhase == 2 &&
            !transitioning &&
            phase3Routine == null &&
            Time.time >= phase2StartedAt + minimumPhase2Duration &&
            GetHealthPercent() <= phase3HealthPercent)
        {
            phase3Routine =
                StartCoroutine(
                    EnterPhase3Routine());
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

    // Los Flight Points viven dentro del prefab del altar, porque el altar
    // es instanciado por TerrainGenerator. El boss recibe esas referencias
    // una vez creado en runtime.
    public void ConfigureFlightPoints(
        Transform[] externalFlightPoints)
    {
        if (externalFlightPoints == null ||
            externalFlightPoints.Length == 0)
        {
            return;
        }

        int validCount = 0;

        for (int i = 0; i < externalFlightPoints.Length; i++)
        {
            if (externalFlightPoints[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return;

        flightPoints = new Transform[validCount];

        int writeIndex = 0;

        for (int i = 0; i < externalFlightPoints.Length; i++)
        {
            if (externalFlightPoints[i] == null)
                continue;

            flightPoints[writeIndex] =
                externalFlightPoints[i];

            writeIndex++;
        }

        // Conservamos el anchor viejo como fallback/primer punto.
        if (flightAnchor == null &&
            flightPoints.Length > 0)
        {
            flightAnchor = flightPoints[0];
        }
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

        Transform initialFlightPoint =
            GetFlightPoint(0);

        if (initialFlightPoint != null)
        {
            transform.position =
                initialFlightPoint.position;

            transform.rotation =
                initialFlightPoint.rotation;
        }
        else if (flightAnchor != null)
        {
            transform.position =
                flightAnchor.position;

            transform.rotation =
                flightAnchor.rotation;
        }

        fightActive = true;
        currentPhase = 1;

        UpdateBossUI();

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Boss iniciado -> Rugido");

        StartCoroutine(BossIntroRoutine());
    }

    private IEnumerator BossIntroRoutine()
    {
        if (!string.IsNullOrWhiteSpace(introRoarAnimation))
            SetAnimation(introRoarAnimation);
        else
            SetAnimation(flightAnimation);

        PlayBossSound(
            introRoarSound,
            introRoarVolume,
            1f,
            CombatVFXPosition,
            introRoarSpatialBlend);

        if (CameraManager.Instance != null &&
            introShakeDuration > 0f &&
            introShakeMagnitude > 0f)
        {
            StartCoroutine(
                IntroShakeRoutine());
        }

        if (introDuration > 0f)
            yield return new WaitForSeconds(introDuration);

        if (!fightActive ||
            hasDied ||
            currentPhase != 1)
        {
            yield break;
        }

        SetAnimation(flightAnimation);
        Log("Fase 1 iniciada");

        phase1FlightLoop =
            StartCoroutine(Phase1FlightRoutine());

        // Vuelo y meteoritos corren en paralelo durante toda la Fase 1.
        meteorLoop =
            StartCoroutine(MeteorLoop());

        RestartCombatLoop();

        // El timer empieza despues del rugido, para no quitar tiempo jugable
        // a la fase de bombardeo.
        StartCoroutine(Phase1Timer());
    }

    private IEnumerator IntroShakeRoutine()
    {
        if (introShakeDelay > 0f)
        {
            yield return new WaitForSeconds(
                introShakeDelay);
        }

        if (!fightActive ||
            hasDied)
        {
            yield break;
        }

        if (CameraManager.Instance == null ||
            introShakeDuration <= 0f ||
            introShakeMagnitude <= 0f)
        {
            yield break;
        }

        CameraManager.Instance.DoScreenShake(
            introShakeDuration,
            introShakeMagnitude);
    }

    private IEnumerator Phase1FlightRoutine()
    {
        int pointCount =
            GetFlightPointCount();

        // Con un solo punto no hay ruta que recorrer, pero el MeteorLoop
        // sigue funcionando normalmente durante toda la Fase 1.
        if (pointCount <= 1)
        {
            while (fightActive &&
                   !hasDied &&
                   currentPhase == 1 &&
                   !transitioning)
            {
                yield return null;
            }

            phase1FlightLoop = null;
            yield break;
        }

        int currentIndex =
            FindClosestFlightPointIndex(
                transform.position);

        while (fightActive &&
               !hasDied &&
               currentPhase == 1 &&
               !transitioning)
        {
            int nextIndex =
                (currentIndex + 1) %
                pointCount;

            Transform destination =
                GetFlightPoint(nextIndex);

            if (destination == null)
            {
                currentIndex = nextIndex;
                yield return null;
                continue;
            }

            // No hay pausa al llegar: cuando termina este tramo,
            // inmediatamente empieza el siguiente.
            yield return StartCoroutine(
                MoveToFlightPoint(
                    destination));

            currentIndex = nextIndex;
        }

        phase1FlightLoop = null;
    }

    private IEnumerator MoveToFlightPoint(
        Transform destination)
    {
        if (destination == null)
            yield break;

        float speed =
            Mathf.Max(
                0.1f,
                phase1FlightSpeed);

        float turn =
            Mathf.Max(
                0.1f,
                phase1FlightTurnSpeed);

        const float arrivalDistance = 0.05f;
        float arrivalDistanceSqr =
            arrivalDistance * arrivalDistance;

        while ((destination.position - transform.position)
               .sqrMagnitude > arrivalDistanceSqr)
        {
            if (!fightActive ||
                hasDied ||
                currentPhase != 1 ||
                transitioning)
            {
                yield break;
            }

            Vector3 direction =
                destination.position -
                transform.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction.normalized,
                        Vector3.up);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        turn * Time.deltaTime);
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    destination.position,
                    speed * Time.deltaTime);

            yield return null;
        }

        // Ajustamos solamente posicion. No forzamos la rotacion del punto,
        // para que el giro continue fluido hacia el siguiente nodo.
        transform.position =
            destination.position;
    }

    private int GetFlightPointCount()
    {
        int count = 0;

        if (flightPoints != null)
        {
            for (int i = 0; i < flightPoints.Length; i++)
            {
                if (flightPoints[i] != null)
                    count++;
            }
        }

        if (count > 0)
            return count;

        return flightAnchor != null
            ? 1
            : 0;
    }

    private Transform GetFlightPoint(
        int validIndex)
    {
        if (validIndex < 0)
            return null;

        int currentValid = 0;

        if (flightPoints != null)
        {
            for (int i = 0; i < flightPoints.Length; i++)
            {
                Transform point =
                    flightPoints[i];

                if (point == null)
                    continue;

                if (currentValid == validIndex)
                    return point;

                currentValid++;
            }
        }

        if (currentValid == 0 &&
            validIndex == 0)
        {
            return flightAnchor;
        }

        return null;
    }

    private int FindClosestFlightPointIndex(
        Vector3 position)
    {
        int pointCount =
            GetFlightPointCount();

        if (pointCount <= 1)
            return 0;

        int bestIndex = 0;
        float bestDistance =
            float.PositiveInfinity;

        for (int i = 0; i < pointCount; i++)
        {
            Transform point =
                GetFlightPoint(i);

            if (point == null)
                continue;

            float sqrDistance =
                (point.position - position)
                .sqrMagnitude;

            if (sqrDistance < bestDistance)
            {
                bestDistance = sqrDistance;
                bestIndex = i;
            }
        }

        return bestIndex;
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

        if (phase1FlightLoop != null)
        {
            StopCoroutine(phase1FlightLoop);
            phase1FlightLoop = null;
        }

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
        lastCombatProgressTime = Time.time;

        // Garantiza que Fase 2 siempre empieza con un loop limpio.
        RestartCombatLoop();

        SetAnimation(idleAnimation);

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 2 iniciada");
    }

    private IEnumerator EnterPhase3Routine()
    {
        if (currentPhase >= 3 ||
            hasDied ||
            !fightActive)
        {
            phase3Routine = null;
            yield break;
        }

        currentPhase = 3;
        transitioning = true;

        // Cortamos cualquier estado de ataque que haya quedado colgado.
        RestartCombatLoop();

        if (dragonAnimator == null)
            dragonAnimator = GetComponentInChildren<Animator>();

        if (dragonAnimator != null)
            dragonAnimator.speed = GetCombatSpeedMultiplier();

        PlayBossSound(
            phase3TransitionSound,
            phase3TransitionVolume,
            1f,
            CombatVFXPosition,
            phase3TransitionSpatialBlend);

        if (enrageVFX != null)
        {
            GameObject vfx =
                Instantiate(
                    enrageVFX,
                    CombatVFXPosition,
                    Quaternion.identity);

            if (enrageVFXLifetime > 0f)
                Destroy(
                    vfx,
                    enrageVFXLifetime);
        }

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 3 iniciada -> Air Assault");

        Transform airAnchor =
            flightAnchor != null
                ? flightAnchor
                : GetFlightPoint(0);

        SetAnimation(flightAnimation);

        if (airAnchor != null)
        {
            yield return StartCoroutine(
                MoveBossToAnchor(
                    airAnchor,
                    phase3TakeoffDuration));
        }

        if (!fightActive ||
            hasDied ||
            currentPhase != 3)
        {
            phase3Routine = null;
            yield break;
        }

        transitioning = false;
        lastCombatProgressTime = Time.time;

        // Arrancamos nuevamente el loop ya posicionados en el aire.
        RestartCombatLoop();

        // Durante este tiempo GroundCombatLoop sólo permite Ranged
        // y MeteorLoop usa la configuración de meteoritos de Fase 3.
        if (phase3AirDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < phase3AirDuration)
            {
                if (!fightActive ||
                    hasDied ||
                    currentPhase != 3)
                {
                    phase3Routine = null;
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        yield return StartCoroutine(
            TransitionToPhase4());

        phase3Routine = null;
    }

    private IEnumerator TransitionToPhase4()
    {
        if (!fightActive ||
            hasDied)
        {
            yield break;
        }

        transitioning = true;
        isAttacking = false;

        Log("Fase 3 terminada -> Volviendo a tierra");

        SetAnimation(landingAnimation);

        if (groundAnchor != null)
        {
            yield return StartCoroutine(
                MoveBossToAnchor(
                    groundAnchor,
                    phase3ReturnDuration));
        }

        if (!fightActive ||
            hasDied)
        {
            yield break;
        }

        currentPhase = 4;
        transitioning = false;
        lastCombatProgressTime = Time.time;

        // La fase final también arranca desde un scheduler limpio.
        RestartCombatLoop();

        if (dragonAnimator == null)
            dragonAnimator = GetComponentInChildren<Animator>();

        if (dragonAnimator != null)
            dragonAnimator.speed = GetCombatSpeedMultiplier();

        SetAnimation(idleAnimation);

        OnPhaseChanged?.Invoke(currentPhase);
        Log("Fase 4 iniciada -> Final Ground Combat");
    }

    private IEnumerator MoveBossToAnchor(
        Transform destination,
        float duration)
    {
        if (destination == null)
            yield break;

        Vector3 startPosition =
            transform.position;

        Quaternion startRotation =
            transform.rotation;

        Vector3 endPosition =
            destination.position;

        Quaternion endRotation =
            destination.rotation;

        if (duration <= 0f)
        {
            transform.position =
                endPosition;

            transform.rotation =
                endRotation;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!fightActive ||
                hasDied)
            {
                yield break;
            }

            float t =
                Mathf.Clamp01(
                    elapsed / duration);

            t =
                t * t *
                (3f - 2f * t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t);

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    endRotation,
                    t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position =
            endPosition;

        transform.rotation =
            endRotation;
    }

    private void RestartCombatLoop()
    {
        if (combatLoop != null)
        {
            StopCoroutine(combatLoop);
            combatLoop = null;
        }

        // El loop nuevo siempre empieza desde un estado neutral.
        isAttacking = false;
        lastCombatProgressTime = Time.time;

        if (!fightActive ||
            hasDied)
        {
            return;
        }

        combatLoop =
            StartCoroutine(
                GroundCombatLoop());
    }

    private void RecoverCombatLoopIfStalled()
    {
        if (!fightActive ||
            hasDied ||
            transitioning ||
            currentPhase < 2)
        {
            return;
        }

        float cooldown =
            currentPhase >= 3
                ? phase3AttackCooldown
                : phase2AttackCooldown;

        float speed =
            GetCombatSpeedMultiplier();

        float longestAttack =
            Mathf.Max(
                clawWindup + clawRecovery,
                slamWindup + slamRecovery,
                projectileWindup + projectileRecovery);

        longestAttack /=
            Mathf.Max(
                1f,
                speed);

        // Dejamos margen suficiente para cualquier ataque normal.
        float timeout =
            Mathf.Max(
                5f,
                cooldown +
                longestAttack +
                2f);

        if (Time.time - lastCombatProgressTime <
            timeout)
        {
            return;
        }

        Log(
            "Combat watchdog: el loop no progreso durante " +
            timeout.ToString("F1") +
            "s. Reiniciando scheduler.");

        RestartCombatLoop();
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

            // Si el ataque terminó correctamente, registramos progreso.
            lastCombatProgressTime = Time.time;
        }
    }

    private IEnumerator PerformGroundAttack()
    {
        if (target == null)
            yield break;

        isAttacking = true;
        lastCombatProgressTime = Time.time;

        FacePlayerImmediate();

        float distance =
            Vector3.Distance(
                transform.position,
                target.position);

        // Fase 3 es exclusivamente aerea:
        // desde el Flight Anchor sólo usa proyectiles.
        if (currentPhase == 3)
        {
            yield return StartCoroutine(
                RangedProjectileAttack());
        }
        // Fase 2 y Fase 4 mantienen el combate terrestre normal.
        else if (distance > rangedAttackDistance)
        {
            yield return StartCoroutine(
                RangedProjectileAttack());
        }
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

        if (currentPhase == 3)
            SetAnimation(flightAnimation);
        else
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

        PlayBossSound(
            rangedAttackSound,
            rangedAttackVolume,
            1f,
            spawnPosition);

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
            if (transitioning ||
                target == null)
            {
                yield return null;
                continue;
            }

            // Fase 1: flujo independiente y constante de meteoritos.
            // El dragon puede estar en mitad de un tramo entre Flight Points.
            if (currentPhase == 1)
            {
                SpawnPhase1Meteor();

                yield return new WaitForSeconds(
                    Mathf.Max(
                        0.1f,
                        phase1MeteorStreamInterval));

                continue;
            }

            if (currentPhase < 2)
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

            yield return StartCoroutine(
                SpawnMeteorVolley(count));

            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnPhase1Meteor()
    {
        if (target == null)
            return;

        Vector3 candidate =
            target.position;

        // Parte de los meteoritos obliga a moverse de la posicion actual;
        // el resto crea presion alrededor para que el patron no sea monotono.
        if (UnityEngine.Random.value >
            phase1TargetedMeteorChance)
        {
            Vector2 random =
                UnityEngine.Random.insideUnitCircle *
                meteorRandomRadius;

            candidate +=
                new Vector3(
                    random.x,
                    0f,
                    random.y);
        }

        Vector3 groundPoint =
            ProjectToGround(candidate);

        CreateMeteorHazard(groundPoint);
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
            meteorVerticalKnockback,
            meteorFallSound,
            meteorImpactSound,
            meteorFallVolume,
            meteorImpactVolume,
            bossAudioSpatialBlend,
            bossAudioMinDistance,
            bossAudioMaxDistance);
    }

    private int GetMeteorCount()
    {
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

        PlayDamageSound();

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
        transitioning = false;
        isAttacking = false;

        if (dragonAnimator != null)
            dragonAnimator.speed = 1f;

        StopAllCoroutines();

        PlayBossSound(
            deathSound,
            deathSoundVolume,
            1f,
            CombatVFXPosition,
            deathSoundSpatialBlend);

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

        // El altar escucha este evento para habilitar la interacción final.
        OnBossDefeated?.Invoke();

        // Importante para Poison/Fire/otros efectos que escuchan OnDeath.
        OnDeath?.Invoke();

        if (deathDisappearDelay <= 0f)
        {
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(
                DisableAfterDeathRoutine());
        }
    }

    private IEnumerator DisableAfterDeathRoutine()
    {
        yield return new WaitForSeconds(
            deathDisappearDelay);

        gameObject.SetActive(false);
    }

    private void PlayDamageSound()
    {
        if (damageSounds == null ||
            damageSounds.Length == 0 ||
            Time.time < nextDamageSoundTime)
        {
            return;
        }

        int validCount = 0;

        for (int i = 0; i < damageSounds.Length; i++)
        {
            if (damageSounds[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return;

        int selectedIndex = -1;

        // Evita repetir exactamente el mismo clip si hay mas de uno disponible.
        if (validCount > 1)
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                int candidate =
                    UnityEngine.Random.Range(
                        0,
                        damageSounds.Length);

                if (damageSounds[candidate] != null &&
                    candidate != lastDamageSoundIndex)
                {
                    selectedIndex = candidate;
                    break;
                }
            }
        }

        if (selectedIndex < 0)
        {
            for (int i = 0; i < damageSounds.Length; i++)
            {
                if (damageSounds[i] != null)
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        if (selectedIndex < 0)
            return;

        lastDamageSoundIndex =
            selectedIndex;

        nextDamageSoundTime =
            Time.time +
            Mathf.Max(
                0f,
                damageSoundCooldown);

        float minimumPitch =
            Mathf.Min(
                damagePitchMin,
                damagePitchMax);

        float maximumPitch =
            Mathf.Max(
                damagePitchMin,
                damagePitchMax);

        float pitch =
            UnityEngine.Random.Range(
                minimumPitch,
                maximumPitch);

        PlayBossSound(
            damageSounds[selectedIndex],
            damageSoundVolume,
            pitch,
            CombatVFXPosition);
    }

    private void PlayBossSound(
        AudioClip clip,
        float volume,
        float pitch,
        Vector3 position)
    {
        PlayBossSound(
            clip,
            volume,
            pitch,
            position,
            bossAudioSpatialBlend);
    }

    private void PlayBossSound(
        AudioClip clip,
        float volume,
        float pitch,
        Vector3 position,
        float spatialBlend)
    {
        if (clip == null ||
            volume <= 0f)
        {
            return;
        }

        GameObject audioObject =
            new GameObject(
                "DragonBoss_Audio_" +
                clip.name);

        audioObject.transform.position =
            position;

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.clip = clip;
        source.volume =
            Mathf.Clamp01(volume);

        source.pitch =
            Mathf.Clamp(
                pitch,
                0.1f,
                3f);

        source.spatialBlend =
            Mathf.Clamp01(
                spatialBlend);

        source.minDistance =
            Mathf.Max(
                0.1f,
                bossAudioMinDistance);

        source.maxDistance =
            Mathf.Max(
                source.minDistance,
                bossAudioMaxDistance);

        source.rolloffMode =
            AudioRolloffMode.Linear;

        source.Play();

        float lifetime =
            clip.length /
            Mathf.Max(
                0.1f,
                Mathf.Abs(source.pitch));

        Destroy(
            audioObject,
            lifetime + 0.15f);
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

        int pointCount =
            GetFlightPointCount();

        for (int i = 0; i < pointCount; i++)
        {
            Transform point =
                GetFlightPoint(i);

            if (point == null)
                continue;

            Gizmos.DrawWireSphere(
                point.position,
                1.25f);

            Transform next =
                GetFlightPoint(
                    (i + 1) %
                    pointCount);

            if (next != null &&
                next != point)
            {
                Gizmos.DrawLine(
                    point.position,
                    next.position);
            }
        }
    }

    private void Log(string message)
    {
        if (logBoss)
            Debug.Log(
                "[FinalDragonBoss] " + message,
                this);
    }
}
