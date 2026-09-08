using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;

public interface IMjolnirRetractBehavior
{
    void OnRetract(Mjolnir mjolnir);
}

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private MjolnirChainController chainController;
    private PlayerContext playerContext;
    private Quaternion startRotation;

    [Header("References")]
    [SerializeField] private Transform hand;

    [Header("Settings")]
    [SerializeField] private float minThrowPower;
    [SerializeField] private float maxThrowPower;
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float minHoldTimeToStartCharge = 0.12f;
    [SerializeField] private float torqueForce;
    [SerializeField] private float maxRetractPower;
    [SerializeField] public float damage;
    //public float parryCooldown;
    // public float nextParryCD;

    public Action<Collider> OnHitEnemy;
    public Action<Collider, Vector3, Vector3, bool> OnMjolnirImpact;
    public Action OnMjolnirThrow;
    public Action OnMjolnirRetract;
    public Action OnChrgingThrow;

    // Eventos de una sola transición. OnMjolnirRetract legacy sigue siendo por tick.
    public event Action OnMjolnirRecallStarted;
    public event Action<BaseEnemy, float, Vector3, Vector3> OnMjolnirRecallHit;
    public event Action<bool> OnMjolnirRecallEnded; // true = atrapado; false = interrumpido

    [Header("Recall hit detection")]
    [SerializeField, Min(0.01f)] private float recallHitRadius = 0.35f;
    [SerializeField] private LayerMask recallObstacleLayer;
    [SerializeField] private bool logRecallDebug;

    // Un enemigo recibe como máximo un impacto por regreso.
    private readonly HashSet<BaseEnemy> recallHitEnemies = new HashSet<BaseEnemy>();
    private readonly List<RecallCandidate> recallCandidates = new List<RecallCandidate>();
    private readonly Collider[] recallColliderBuffer = new Collider[256];
    private float recallDamageMultiplier = 1f;
    private float recallSpeedMultiplier = 1f;

    public float RecallDamageMultiplier => recallDamageMultiplier;
    public float RecallSpeedMultiplier => recallSpeedMultiplier;

    private struct RecallCandidate
    {
        public BaseEnemy Enemy;
        public Collider Collider;
        public float Along;
        public Vector3 Point;
        public Vector3 Normal;
    }

    public void ConfigureRecallModifiers(float damageMultiplier, float speedMultiplier)
    {
        recallDamageMultiplier = Mathf.Max(0f, damageMultiplier);
        recallSpeedMultiplier = Mathf.Max(0.01f, speedMultiplier);
    }

    public void ResetRecallModifiers()
    {
        recallDamageMultiplier = 1f;
        recallSpeedMultiplier = 1f;
    }

    private bool isHeld;
    private bool isRetracting;
    public bool IsRetracting => isRetracting;

    public bool IsChargingThrow => isChargingThrow;
    public bool teleport;
    public bool parry;

    private float throwChargeTime = 0f;
    private float pressHoldTime;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;
    private Vector3 originalSize;

    // --- VFX on hit (add-only) ---
    [Header("VFX")]
    [SerializeField] private GameObject hitVFXPrefab;   // Prefab with ParticleSystem or VFX Graph
    [SerializeField] private float hitVFXLifetime = 2f; // Safety destroy time
    [SerializeField] private bool parentVFXToHit = false; // Stick effect to the hit object
    private readonly List<IMjolnirRetractBehavior> retractBehaviors = new();
    public void RegisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (!retractBehaviors.Contains(behavior))
            retractBehaviors.Add(behavior);
    }
    public void UnregisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (retractBehaviors.Contains(behavior))
            retractBehaviors.Remove(behavior);
    }


    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        chainController = GetComponent<MjolnirChainController>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
        originalSize = transform.localScale;
        Catch();
    }


    private void Update()
    {
        bool isThrowButtonHeld =
            playerContext.HandleInputs.IsThrowing();

        // ==========================================
        // THROW INPUT
        // ==========================================

        if (isHeld && isThrowButtonHeld)
        {
            if (!isChargingThrow)
            {
                pressHoldTime += Time.deltaTime;

                // después del threshold empieza la carga.
                if (pressHoldTime >= minHoldTimeToStartCharge)
                {
                    StartChargingThrow();
                }
            }
            else
            {
                // Ahora sí cuenta como carga real.
                throwChargeTime += Time.deltaTime;

                throwChargeTime = Mathf.Min(
                    throwChargeTime,
                    maxChargeTime
                );
            }
        }

        // Soltó el botón
        if (wasThrowing && !isThrowButtonHeld)
        {
            if (isChargingThrow)
            {
                // Sólo lanzamos si REALMENTE había empezado la carga.
                Throw();
            }

            ResetThrowInput();
        }

        wasThrowing = isThrowButtonHeld;

        // ==========================================
        // CATCH
        // ==========================================

        if (!isHeld &&
            !isRetracting &&
            playerContext.HandleInputs.IsCatching())
        {
            BeginRetract();
        }
    }

    private void FixedUpdate()
    {
        if (isRetracting)
        {
            Retract();
        }
    }

    private void ResetThrowInput()
    {
        pressHoldTime = 0f;
        throwChargeTime = 0f;
        isChargingThrow = false;
    }
    private void StartChargingThrow()
    {
        isChargingThrow = true;
        throwChargeTime = 0f;
    }
    public void Throw()
    {
        float charge01 = Mathf.Clamp01(throwChargeTime / Mathf.Max(0.01f, maxChargeTime));
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, charge01);
        PrepareThrow();
        rb.AddForce(SearchForCloseEnemies() * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    private void PrepareThrow()
    {
        // Un relanzamiento (por ejemplo, Parry) termina el regreso anterior.
        if (isRetracting)
            OnMjolnirRecallEnded?.Invoke(false);
        isRetracting = false;
        recallHitEnemies.Clear();
        if (chainController == null) chainController = GetComponent<MjolnirChainController>();
        if (chainController != null) chainController.BeginFlight();
        OnMjolnirThrow?.Invoke();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isHeld = false;
        transform.parent = null;
    }

    public Vector3 SearchForCloseEnemies()
    {
        float searchRadius = 40f;
        float maxAngle = 15f; // Ángulo del cono de visión, en grados
        LayerMask enemyMask = LayerMask.GetMask("HammerTarget");

        // Obtener enemigos en un radio general
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyMask);

        Transform bestTarget = null;
        float bestScore = -1f;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraPosition = Camera.main.transform.position;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitWAim, 60f, enemyMask))
        {
            return (hitWAim.transform.position - transform.position).normalized;
        }

        foreach (var hit in hits)
        {
            Vector3 toEnemy = (hit.transform.position - cameraPosition).normalized;
            float angle = Vector3.Angle(cameraForward, toEnemy);

            // Ignorar enemigos fuera del cono frontal
            if (angle > maxAngle) continue;

            // Calcular una "puntuación" de prioridad: más cerca y más centrado
            float distance = Vector3.Distance(cameraPosition, hit.transform.position);
            float score = Mathf.Lerp(1f, 0f, angle / maxAngle) / distance;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget != null)
        {
            // Direccion hacia el objetivo elegido
            Vector3 directionToTarget = (bestTarget.position - transform.position).normalized;
            return directionToTarget;
        }

        // Si no hay enemigos válidos, lanzar hacia adelante
        return cameraForward;
    }


    public void ThrowWithPower(float powerMultiplier = 1f)
    {
        PrepareThrow();
        Vector3 cameraForward = Camera.main.transform.forward;
        float finalThrowPower = maxThrowPower * powerMultiplier;
        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    public void ShowMessage(string message) { Debug.Log(message); }

    private void Retract()
    {
        if (isHeld) return;
        if (chainController != null && chainController.IsTraveling) return;

        // Conservamos los comportamientos legacy existentes.
        OnMjolnirRetract?.Invoke();
        foreach (var behavior in retractBehaviors)
            behavior.OnRetract(this);

        if (isHeld || !isRetracting) return;

        Vector3 directionToHand = hand.position - transform.position;
        if (directionToHand.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToHand);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,
                Time.fixedDeltaTime * 5f);
        }

        // El Recall vuelve a mover el Transform directamente, como en la
        // versión estable de Helvegr. No dejamos un MovePosition pendiente
        // que pueda sobrescribir el snap del Catch al terminar la física.
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        Vector3 start = transform.position;
        Vector3 end = Vector3.MoveTowards(start, hand.position,
            maxRetractPower * recallSpeedMultiplier * Time.fixedDeltaTime);

        // Storm Recall conserva el barrido antes de mover. Los callbacks pueden
        // cancelar el regreso o atrapar el martillo, por eso comprobamos ambos.
        SweepRecall(start, end);
        if (isHeld || !isRetracting) return;

        transform.position = end;
        if (Vector3.Distance(hand.position, transform.position) < 0.5f)
        {
            Catch();
            return;
        }

        transform.localScale = originalSize;
    }

    private void SweepRecall(Vector3 start, Vector3 end)
    {
        recallCandidates.Clear();
        int count = Physics.OverlapCapsuleNonAlloc(start, end, recallHitRadius,
            recallColliderBuffer, ~0, QueryTriggerInteraction.Ignore);

        // Si se llena el buffer, hacemos una consulta completa para no perder enemigos.
        if (count == recallColliderBuffer.Length)
        {
            Collider[] all = Physics.OverlapCapsule(start, end, recallHitRadius,
                ~0, QueryTriggerInteraction.Ignore);
            foreach (Collider collider in all)
                AddRecallCandidate(collider, start, end);
        }
        else
        {
            for (int i = 0; i < count; i++)
                AddRecallCandidate(recallColliderBuffer[i], start, end);
        }

        // Lv4 depende del orden: primero el enemigo más cercano al inicio del tramo.
        recallCandidates.Sort(CompareRecallCandidates);
        foreach (RecallCandidate candidate in recallCandidates)
        {
            if (isHeld || !isRetracting) break;
            BaseEnemy enemy = candidate.Enemy;
            if (enemy == null || enemy.IsDead() || !enemy.gameObject.activeInHierarchy)
                continue;
            if (recallHitEnemies.Contains(enemy)) continue;
            if (IsRecallPathBlocked(start, candidate.Point)) continue;

            // Se marca ANTES de disparar eventos que podrían provocar otros impactos.
            recallHitEnemies.Add(enemy);
            ResolveRecallImpact(candidate);
        }
        recallCandidates.Clear();
    }

    private void AddRecallCandidate(Collider collider, Vector3 start, Vector3 end)
    {
        if (collider == null || !collider.enabled || collider.isTrigger ||
            !collider.gameObject.activeInHierarchy || collider.transform.IsChildOf(transform) ||
            IsPlayerCollider(collider)) return;

        // No exigimos que el root y los colliders tengan la misma layer.
        BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
        if (enemy == null || enemy.IsDead() || !enemy.gameObject.activeInHierarchy ||
            recallHitEnemies.Contains(enemy)) return;

        Vector3 segment = end - start;
        float length = segment.magnitude;
        Vector3 direction = length > 0.0001f ? segment / length : Vector3.forward;
        Vector3 closest = collider.ClosestPoint(start);
        float along = length > 0.0001f
            ? Mathf.Clamp(Vector3.Dot(closest - start, direction), 0f, length) : 0f;
        Vector3 axisPoint = start + direction * along;
        Vector3 point = collider.ClosestPoint(axisPoint);
        Vector3 normal = axisPoint - point;
        if (normal.sqrMagnitude < 0.0001f) normal = -direction;

        recallCandidates.Add(new RecallCandidate
        {
            Enemy = enemy,
            Collider = collider,
            Along = along,
            Point = point,
            Normal = normal.normalized
        });
    }

    private static int CompareRecallCandidates(RecallCandidate a, RecallCandidate b)
    {
        int order = a.Along.CompareTo(b.Along);
        return order != 0 ? order : a.Enemy.GetInstanceID().CompareTo(b.Enemy.GetInstanceID());
    }

    private bool IsRecallPathBlocked(Vector3 origin, Vector3 goal)
    {
        Vector3 delta = goal - origin;
        float distance = delta.magnitude;
        if (recallObstacleLayer.value == 0 || distance < 0.02f) return false;
        RaycastHit[] hits = Physics.RaycastAll(origin, delta / distance, distance,
            recallObstacleLayer, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            Collider collider = hit.collider;
            if (collider == null || collider.transform.IsChildOf(transform) ||
                IsPlayerCollider(collider) || collider.GetComponentInParent<BaseEnemy>() != null)
                continue;
            if (hit.distance > 0.01f) return true;
        }
        return false;
    }

    private void ResolveRecallImpact(RecallCandidate candidate)
    {
        BaseEnemy enemy = candidate.Enemy;
        if (enemy == null || enemy.IsDead()) return;

        // Capturamos posiciones y daño antes de que un VFX/AoE pueda matar al objetivo.
        Vector3 visualPosition = enemy.CombatVFXPosition;
        Vector3 gameplayPosition = enemy.transform.position;
        float hitDamage = Mathf.Max(0f, damage * recallDamageMultiplier);

        // Compatibilidad con Draugblót y los powerups existentes.
        OnMjolnirImpact?.Invoke(candidate.Collider, candidate.Point, candidate.Normal, true);
        OnMjolnirRecallHit?.Invoke(enemy, hitDamage, visualPosition, gameplayPosition);

        // El multiplicador del siguiente golpe ya pudo cambiar, pero hitDamage
        // conserva el valor de ESTE impacto. No aplicamos una segunda hit reaction.
        if (enemy != null && !enemy.IsDead() && hitDamage > 0f)
            enemy.TakeEffectDamage(hitDamage, DamageFeedbackType.Normal);

        OnHitEnemy?.Invoke(candidate.Collider);
        SpawnMjolnirHitEffect(candidate.Point, candidate.Normal, candidate.Collider.transform);
        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.PlaySound("MjolnirThrowHit");
        if (logRecallDebug)
            Debug.Log("[Mjolnir Recall] Impacto: " + enemy.name + " | daño: " + hitDamage, this);
    }

    public void Catch()
    {
        bool wasRetracting = isRetracting;
        isHeld = true;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        isRetracting = false;
        if (chainController != null) chainController.Cancel();
        // Snap físico y visual explícito. No debe quedar ningún movimiento
        // cinemático pendiente después de volver a ser hijo de la mano.
        rb.position = hand.position;
        transform.SetParent(hand, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = startRotation;
        transform.localScale = originalSize;
        recallHitEnemies.Clear();
        if (wasRetracting)
            OnMjolnirRecallEnded?.Invoke(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Durante Recall el barrido es la única fuente de impactos.
        if (isHeld || isRetracting || (chainController != null && chainController.IsTraveling)) return;
        Vector3 hitPoint;
        Vector3 hitNormal;
        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.GetContact(0);
            hitPoint = contact.point;
            hitNormal = contact.normal.normalized;
        }
        else
        {
            hitPoint = collision.collider.ClosestPoint(transform.position);
            hitNormal = transform.position - collision.collider.bounds.center;
            if (hitNormal.sqrMagnitude < 0.001f) hitNormal = -transform.forward;
            hitNormal.Normalize();
        }
        ResolveDamageableImpact(collision.collider, hitPoint, hitNormal);
    }

    // Usado por el sweep de Helvegr. El evento es el mismo que en un impacto físico.
    public void ResolveChainImpact(Collider collider, Vector3 point, Vector3 normal)
    {
        if (isHeld || isRetracting || chainController == null || !chainController.IsTraveling) return;
        ResolveDamageableImpact(collider, point, normal);
    }

    private void ResolveDamageableImpact(Collider collider, Vector3 point, Vector3 normal)
    {
        if (collider == null) return;
        IDamageable damageable = collider.GetComponentInParent<IDamageable>();
        if (damageable == null || IsPlayerCollider(collider)) return;
        BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
        if (enemy != null && enemy.IsDead()) return;
        bool wasRecallHit = isRetracting;
        Vector3 enemyCenter = enemy != null ? enemy.CombatVFXPosition : point;
        Vector3 enemyGameplayPosition = enemy != null ? enemy.transform.position : point;

        // Helvegr reclama el impacto ANTES de que el daño o los eventos
        // puedan matar/desactivar al enemigo o iniciar el auto-recall.
        // Un recall pedido por el jugador sigue pudiendo cancelar la cadena.
        bool chainHandled = enemy != null && chainController != null &&
            chainController.TryHandleImpact(enemy, enemyCenter, enemyGameplayPosition, wasRecallHit);

        // Conservamos el contrato existente: VFX antes del daño base.
        OnMjolnirImpact?.Invoke(collider, point, normal, wasRecallHit);
        if (enemy == null || !enemy.IsDead()) damageable.TakeDamage(damage);
        OnHitEnemy?.Invoke(collider);
        SpawnMjolnirHitEffect(point, normal, collider.transform);
        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.PlaySound("MjolnirThrowHit");

        // Sólo el primer impacto sin cadena inicia el retorno automático.
        if (enemy != null && !chainHandled && !isRetracting)
            BeginRetract(true);
    }

    public bool IsPlayerCollider(Collider collider)
    {
        return collider != null && playerContext != null &&
            collider.GetComponentInParent<PlayerContext>() == playerContext;
    }

    public void SetChainController(MjolnirChainController controller)
    {
        chainController = controller;
    }

    public void BeginRetract(bool playCatchAnimation = false)
    {
        if (isHeld) return;
        bool starting = !isRetracting;
        isRetracting = true;
        if (chainController != null) chainController.Cancel();
        if (starting)
        {
            recallHitEnemies.Clear();
            OnMjolnirRecallStarted?.Invoke();
        }
        if (playCatchAnimation && playerContext != null)
            playerContext.PlayerStateMachine.ChangeState(playerContext.PlayerStateMachine.catchingState);
    }

    private void SpawnMjolnirHitEffect(Vector3 point, Vector3 normal, Transform hitTransform)
    {
        if (hitVFXPrefab == null) return;
        if (normal.sqrMagnitude < 0.001f) normal = -transform.forward;
        Vector3 position = point + normal.normalized * 0.08f;
        GameObject vfx = Instantiate(hitVFXPrefab, position, Quaternion.identity);
        if (parentVFXToHit && hitTransform != null) vfx.transform.SetParent(hitTransform, true);
        if (hitVFXLifetime > 0f) Destroy(vfx, hitVFXLifetime);
    }
    public void StopRetracting()
    {
        bool wasRetracting = isRetracting;
        isRetracting = false;
        recallHitEnemies.Clear();
        if (chainController != null && chainController.IsTraveling) chainController.Cancel();
        if (wasRetracting)
            OnMjolnirRecallEnded?.Invoke(false);
    }

    public bool IsHeld() { return isHeld; }

    public float DistanceFromHand() { return Vector3.Distance(hand.position, transform.position); }
}
