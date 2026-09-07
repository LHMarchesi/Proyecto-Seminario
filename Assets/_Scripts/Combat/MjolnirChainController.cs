using System.Collections.Generic;
using UnityEngine;

// Unico propietario del movimiento durante Helvegr. No crea clones de Mjolnir.
[DisallowMultipleComponent]
[RequireComponent(typeof(Mjolnir), typeof(Rigidbody))]
public class MjolnirChainController : MonoBehaviour
{
    private Mjolnir hammer;
    private Rigidbody body;
    private HelvegrPowerUp owner;
    private LayerMask targetMask;
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
        targetMask = enemies;
        obstacleMask = obstacles;
        configuredJumps = Mathf.Max(1, jumps);
        searchRange = Mathf.Max(0.1f, range);
        speed = Mathf.Max(1f, travelSpeed);
        castRadius = Mathf.Max(0.01f, radius);
        arrivalDistance = Mathf.Max(0.01f, arrival);
        hammer.SetChainController(this);
        enabled = true;
        // Un item adquirido a mitad de un vuelo no secuestra ese vuelo.
        if (!flightArmed && !traveling) visited.Clear();
    }

    public void BeginFlight()
    {
        Cancel();
        if (!IsConfigured || !enabled) return;
        remainingJumps = configuredJumps;
        flightArmed = true;
    }

    public void Cancel()
    {
        bool wasTraveling = traveling;
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        visited.Clear();
        // Si se cancela sin iniciar Recall, devolvemos el control a la física normal.
        if (wasTraveling && body != null && hammer != null &&
            !hammer.IsHeld() && !hammer.IsRetracting)
            body.isKinematic = false;
    }

    public void Release()
    {
        if (traveling && hammer != null && !hammer.IsHeld())
            hammer.BeginRetract(true);
        Cancel();
        owner = null;
        enabled = false;
    }

    // Devuelve true si Helvegr toma el control y no debe iniciarse el auto-recall normal.
    // Se llama después del daño base, con el punto visual capturado antes del daño.
    public bool TryHandleImpact(BaseEnemy enemy, Vector3 impactPosition,
        Vector3 gameplayPosition, bool isRecallHit)
    {
        if (!flightArmed || !IsConfigured || !enabled || isRecallHit || hammer.IsRetracting)
            return false;
        if (enemy == null) return traveling;

        int id = enemy.GetInstanceID();
        if (!visited.Add(id)) return traveling;

        if (traveling)
        {
            remainingJumps = Mathf.Max(0, remainingJumps - 1);
            owner.OnChainImpact(enemy, impactPosition, gameplayPosition);
        }

        if (remainingJumps > 0 && SelectNextTarget(impactPosition))
        {
            traveling = true;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            return true;
        }

        // No hay más saltos: Mjolnir vuelve por su sistema normal.
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        return false;
    }

    private bool SelectNextTarget(Vector3 origin)
    {
        currentTarget = null;
        currentTargetCollider = null;
        List<BaseEnemy> candidates = CombatAreaDamage.FindTargets(origin, searchRange, targetMask);
        foreach (BaseEnemy candidate in candidates)
        {
            if (candidate == null || visited.Contains(candidate.GetInstanceID())) continue;
            Collider collider = FindBodyCollider(candidate, origin);
            if (collider == null) continue;
            // El mask de obstáculos debe contener SOLO escenario, no enemigos.
            if (obstacleMask.value != 0 && Physics.Linecast(origin,
                candidate.CombatVFXPosition, obstacleMask, QueryTriggerInteraction.Ignore)) continue;
            currentTarget = candidate;
            currentTargetCollider = collider;
            return true;
        }
        return false;
    }

    private Collider FindBodyCollider(BaseEnemy enemy, Vector3 origin)
    {
        Collider best = null;
        float bestDistance = float.PositiveInfinity;
        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
        {
            if (collider == null || !collider.enabled || collider.isTrigger ||
                collider.GetComponentInParent<BaseEnemy>() != enemy ||
                (targetMask.value & (1 << collider.gameObject.layer)) == 0) continue;
            float distance = (collider.ClosestPoint(origin) - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = collider;
            }
        }
        return best;
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
            if (!SelectNextTarget(body.position)) FinishChain();
            return;
        }

        Vector3 origin = body.position;
        Vector3 goal = currentTargetCollider.ClosestPoint(origin);
        Vector3 delta = goal - origin;
        if (delta.magnitude <= arrivalDistance)
        {
            ResolveTarget(goal);
            return;
        }
        Vector3 direction = delta.normalized;
        float distance = Mathf.Min(speed * Time.fixedDeltaTime, delta.magnitude);

        // El sweep evita atravesar enemigos o paredes entre dos FixedUpdates.
        int mask = targetMask.value | obstacleMask.value;
        RaycastHit[] hits = Physics.SphereCastAll(origin, castRadius, direction,
            distance + 0.01f, mask, QueryTriggerInteraction.Ignore);
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
                if (enemy.IsDead() || visited.Contains(enemy.GetInstanceID())) continue;
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
            // Avanzamos hasta el contacto, no atravesamos el objeto.
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
        traveling = false;
        flightArmed = false;
        currentTarget = null;
        currentTargetCollider = null;
        if (hammer != null && !hammer.IsHeld()) hammer.BeginRetract(true);
    }

    private void OnDisable() { Cancel(); }
}
