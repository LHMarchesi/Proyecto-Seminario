using System.Collections.Generic;
using UnityEngine;

// Unico propietario del movimiento durante Helvegr. No crea clones de Mjolnir.
[DisallowMultipleComponent]
[RequireComponent(typeof(Mjolnir), typeof(Rigidbody))]
public class MjolnirChainController : MonoBehaviour
{
    [Header("Diagnostics")]
    [SerializeField] private bool logChainDebug;

    private Mjolnir hammer;
    private Rigidbody body;
    private HelvegrPowerUp owner;
    private LayerMask obstacleMask;
    private int remainingJumps;
    private int configuredJumps;
    private float searchRange;
    private float speed;
    private float castRadius;
    private float arrivalDistance;
    private bool flightArmed;
    private bool traveling;
    private BaseEnemy currentTarget;
    private Collider currentTargetCollider;
    private readonly HashSet<int> visited = new HashSet<int>();

    public bool IsTraveling => traveling;
    public bool IsConfigured => owner != null;
    public bool IsFlightArmed => flightArmed;

    private void Awake()
    {
        hammer = GetComponent<Mjolnir>();
        body = GetComponent<Rigidbody>();
    }

    public void Configure(HelvegrPowerUp powerUp, LayerMask enemies, LayerMask obstacles,
        int jumps, float range, float travelSpeed, float radius, float arrival)
    {
        if (hammer == null) hammer = GetComponent<Mjolnir>();
        if (body == null) body = GetComponent<Rigidbody>();
        owner = powerUp;
        // La búsqueda valida BaseEnemy, incluso si sus colliders usan otras layers.
        if (enemies.value == 0) Log("Enemy Layer vacío: se buscarán igualmente componentes BaseEnemy");
        obstacleMask = obstacles;
        configuredJumps = Mathf.Max(1, jumps);
        searchRange = Mathf.Max(0.1f, range);
        speed = Mathf.Max(1f, travelSpeed);
        castRadius = Mathf.Max(0.01f, radius);
        arrivalDistance = Mathf.Max(0.01f, arrival);
        hammer.SetChainController(this);
        enabled = true;
        Log("Configurado: " + configuredJumps + " saltos, rango " + searchRange);
    }

    public void BeginFlight()
    {
        Cancel();
        if (!IsConfigured || !enabled) return;
        remainingJumps = configuredJumps;
        flightArmed = true;
        Log("Lanzamiento armado");
    }

    public void Cancel()
    {
        bool wasTraveling = traveling;
        bool wasArmed = flightArmed;
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        visited.Clear();
        remainingJumps = 0;

        // No devolvemos la física dinámica cuando el recall ya tomó el control.
        if (wasTraveling && body != null && hammer != null &&
            !hammer.IsHeld() && !hammer.IsRetracting)
            body.isKinematic = false;

        if (wasTraveling || wasArmed) Log("Cadena cancelada");
    }

    public void Release()
    {
        if (traveling && hammer != null && !hammer.IsHeld())
            hammer.BeginRetract(true);
        Cancel();
        owner = null;
        enabled = false;
    }

    // True significa que Helvegr se hace cargo del impacto y Mjolnir NO debe
    // comenzar su retorno normal. Se llama antes del daño base.
    public bool TryHandleImpact(BaseEnemy enemy, Vector3 impactPosition,
        Vector3 gameplayPosition, bool isRecallHit)
    {
        if (!flightArmed || !IsConfigured || !enabled || isRecallHit ||
            hammer == null || hammer.IsRetracting)
            return false;
        if (enemy == null) return traveling;

        int id = enemy.GetInstanceID();
        if (!visited.Add(id)) return traveling;

        if (traveling)
        {
            remainingJumps = Mathf.Max(0, remainingJumps - 1);
            owner.OnChainImpact(enemy, impactPosition, gameplayPosition);
        }

        Log("Impacto: " + enemy.name + "; saltos restantes: " + remainingJumps);

        if (remainingJumps > 0 && SelectNextTarget(impactPosition))
        {
            traveling = true;
            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }
            return true;
        }

        // El llamador conserva el auto-recall normal cuando no hay otro objetivo.
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        Log("Sin más saltos: retorno normal");
        return false;
    }

    private bool SelectNextTarget(Vector3 origin)
    {
        currentTarget = null;
        currentTargetCollider = null;
        BaseEnemy bestEnemy = null;
        Collider bestCollider = null;
        float bestDistance = float.PositiveInfinity;
        int bestId = int.MaxValue;
        var unique = new HashSet<BaseEnemy>();

        // Buscamos colliders de todas las layers, pero sólo aceptamos BaseEnemy.
        // El mask del item puede estar en el root/trigger y el cuerpo físico en
        // otra layer. Exigir la misma layer a ambos dejaba la cadena sin objetivo.
        // El parámetro enemies de Configure se conserva para compatibilidad.
        Collider[] hits = Physics.OverlapSphere(origin, searchRange, ~0,
            QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            BaseEnemy candidate = hit.GetComponentInParent<BaseEnemy>();
            if (candidate == null || candidate.IsDead() ||
                !candidate.gameObject.activeInHierarchy ||
                visited.Contains(candidate.GetInstanceID()) || !unique.Add(candidate))
                continue;

            Collider physical = FindBodyCollider(candidate, origin);
            if (physical == null) continue;
            Vector3 goal = GetGoal(physical, origin);
            float distance = (goal - origin).sqrMagnitude;
            if (distance > searchRange * searchRange) continue;
            if (HasObstacle(origin, goal)) continue;

            int id = candidate.GetInstanceID();
            if (distance < bestDistance || (Mathf.Approximately(distance, bestDistance) && id < bestId))
            {
                bestDistance = distance;
                bestId = id;
                bestEnemy = candidate;
                bestCollider = physical;
            }
        }

        currentTarget = bestEnemy;
        currentTargetCollider = bestCollider;
        if (bestEnemy != null)
            Log("Siguiente objetivo: " + bestEnemy.name);
        else
            Log("No se encontró otro BaseEnemy físico dentro del rango y sin obstáculos");
        return bestEnemy != null;
    }

    private Collider FindBodyCollider(BaseEnemy enemy, Vector3 origin)
    {
        Collider best = null;
        float bestDistance = float.PositiveInfinity;
        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
        {
            if (collider == null || !collider.enabled || collider.isTrigger ||
                !collider.gameObject.activeInHierarchy ||
                collider.GetComponentInParent<BaseEnemy>() != enemy)
                continue;
            float distance = (collider.ClosestPoint(origin) - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = collider;
            }
        }
        return best;
    }

    private static Vector3 GetGoal(Collider collider, Vector3 origin)
    {
        Vector3 point = collider.ClosestPoint(origin);
        // ClosestPoint devuelve origin si estamos dentro del collider.
        if ((point - origin).sqrMagnitude < 0.0001f)
            return collider.bounds.center;
        return point;
    }

    private bool HasObstacle(Vector3 origin, Vector3 goal)
    {
        Vector3 delta = goal - origin;
        float distance = delta.magnitude;
        if (obstacleMask.value == 0 || distance < 0.02f) return false;
        RaycastHit[] hits = Physics.RaycastAll(origin, delta / distance, distance,
            obstacleMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            Collider collider = hit.collider;
            if (collider == null || collider.transform.IsChildOf(transform) ||
                hammer.IsPlayerCollider(collider) ||
                collider.GetComponentInParent<BaseEnemy>() != null)
                continue;
            if (hit.distance > 0.01f) return true;
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (!traveling || body == null || hammer == null) return;
        if (hammer.IsHeld() || hammer.IsRetracting)
        {
            Cancel();
            return;
        }
        if (currentTarget == null || currentTarget.IsDead() ||
            !currentTarget.gameObject.activeInHierarchy || currentTargetCollider == null ||
            !currentTargetCollider.enabled)
        {
            if (remainingJumps <= 0 || !SelectNextTarget(body.position)) FinishChain();
            return;
        }

        Vector3 origin = body.position;
        Vector3 goal = GetGoal(currentTargetCollider, origin);
        Vector3 delta = goal - origin;
        if (delta.magnitude <= arrivalDistance)
        {
            ResolveTarget(goal);
            return;
        }
        Vector3 direction = delta.normalized;
        float distance = Mathf.Min(speed * Time.fixedDeltaTime, delta.magnitude);

        // Sweep con todas las layers: los enemigos se identifican por BaseEnemy;
        // obstacleMask sólo decide qué superficies detienen el movimiento.
        RaycastHit[] hits = Physics.SphereCastAll(origin, castRadius, direction,
            distance + 0.01f, ~0, QueryTriggerInteraction.Ignore);
        RaycastHit nearest = new RaycastHit();
        bool found = false;
        float nearestDistance = float.PositiveInfinity;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hammer.IsPlayerCollider(hit.collider)) continue;
            BaseEnemy enemy = hit.collider.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                if (enemy.IsDead() || !enemy.gameObject.activeInHierarchy ||
                    visited.Contains(enemy.GetInstanceID())) continue;
            }
            else if ((obstacleMask.value & (1 << hit.collider.gameObject.layer)) == 0)
                continue;
            if (hit.distance < nearestDistance)
            {
                nearest = hit;
                nearestDistance = hit.distance;
                found = true;
            }
        }
        if (found)
        {
            body.MovePosition(origin + direction * Mathf.Max(0f, nearest.distance - 0.01f));
            BaseEnemy enemy = nearest.collider.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                Vector3 point = nearest.distance <= 0.0001f
                    ? nearest.collider.ClosestPoint(origin) : nearest.point;
                hammer.ResolveChainImpact(nearest.collider, point, nearest.normal);
            }
            else
                FinishChain();
            return;
        }

        body.MovePosition(origin + direction * distance);
        if (distance >= delta.magnitude - arrivalDistance)
            ResolveTarget(goal);
    }

    private void ResolveTarget(Vector3 point)
    {
        if (currentTargetCollider == null) { FinishChain(); return; }
        Vector3 normal = body.position - currentTargetCollider.bounds.center;
        if (normal.sqrMagnitude < 0.0001f) normal = -transform.forward;
        hammer.ResolveChainImpact(currentTargetCollider, point, normal.normalized);
    }

    private void FinishChain()
    {
        if (!traveling) return;
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        Log("Cadena terminada: recall");
        if (hammer != null && !hammer.IsHeld()) hammer.BeginRetract(true);
    }

    private void Log(string message)
    {
        if (logChainDebug) Debug.Log("Helvegr: " + message, this);
    }

    private void OnDisable() { Cancel(); }
}
