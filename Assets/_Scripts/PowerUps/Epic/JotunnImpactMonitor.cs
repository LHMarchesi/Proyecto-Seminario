using System.Collections.Generic;
using UnityEngine;

// Detecta un contacto simplificado durante el desplazamiento de un enemigo
// lanzado. No modifica la física ni hace que las explosiones generen más lanzamientos.
[DisallowMultipleComponent]
public class JotunnImpactMonitor : MonoBehaviour
{
    private JotunnBreakerPowerUp owner;
    private BaseEnemy source;
    private Rigidbody body;
    private LayerMask enemyLayer;
    private float damage;
    private float deadline;
    private float probeRadius;
    private float minimumTravel;
    private float minimumSpeed;
    private Vector3 startPosition;
    private Vector3 lastPosition;
    private Vector3 launchDirection;
    private bool armed;
    private readonly HashSet<BaseEnemy> initialNeighbors = new HashSet<BaseEnemy>();

    public void Arm(JotunnBreakerPowerUp powerUp, BaseEnemy launchedEnemy,
        Rigidbody launchedBody, Vector3 direction, float impactDamage,
        LayerMask targets, float window, float radius, float travel, float speed)
    {
        ResetMonitor();
        if (powerUp == null || launchedEnemy == null || launchedBody == null ||
            launchedBody.isKinematic || impactDamage <= 0f) return;

        owner = powerUp;
        source = launchedEnemy;
        body = launchedBody;
        enemyLayer = targets;
        damage = impactDamage;
        deadline = Time.time + Mathf.Max(0.05f, window);
        probeRadius = Mathf.Max(0.01f, radius);
        minimumTravel = Mathf.Max(0f, travel);
        minimumSpeed = Mathf.Max(0f, speed);
        launchDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized : Vector3.forward;
        launchDirection.y = 0f;
        launchDirection.Normalize();
        startPosition = body.worldCenterOfMass;
        lastPosition = startPosition;

        // No contamos como choque nuevo a los enemigos que ya estaban tocando
        // al lanzado en el momento del Charged.
        Collider[] nearby = Physics.OverlapSphere(startPosition, probeRadius,
            enemyLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider collider in nearby)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy != null) initialNeighbors.Add(enemy);
        }
        initialNeighbors.Add(source);
        armed = true;
        enabled = true;
    }

    private void FixedUpdate()
    {
        if (!armed) return;
        if (owner == null || !owner.isActiveAndEnabled || source == null || body == null ||
            !source.gameObject.activeInHierarchy || source.IsDead() ||
            body.isKinematic || Time.time >= deadline)
        {
            ResetMonitor();
            return;
        }

        Vector3 current = body.worldCenterOfMass;
        Vector3 previous = lastPosition;
        lastPosition = current;
        Vector3 movement = current - previous;

        // Exigimos desplazamiento real en la dirección del lanzamiento.
        if (Vector3.Dot(current - startPosition, launchDirection) < minimumTravel ||
            movement.sqrMagnitude < 0.000001f ||
            Vector3.Dot(movement, launchDirection) < minimumSpeed * Time.fixedDeltaTime)
            return;

        Collider[] contacts = Physics.OverlapCapsule(previous, current, probeRadius,
            enemyLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider collider in contacts)
        {
            BaseEnemy target = collider.GetComponentInParent<BaseEnemy>();
            if (target == null || target.IsDead() ||
                !target.gameObject.activeInHierarchy || initialNeighbors.Contains(target))
                continue;

            Vector3 point = collider.ClosestPoint(current);
            if ((point - current).sqrMagnitude < 0.0001f)
                point = collider.bounds.center;

            JotunnBreakerPowerUp effect = owner;
            BaseEnemy launched = source;
            float impactDamage = damage;
            ResetMonitor(); // Antes del AoE: un impacto por lanzamiento.
            if (effect != null)
                effect.OnLaunchedEnemyImpact(launched, point, impactDamage);
            return;
        }
    }

    private void ResetMonitor()
    {
        armed = false;
        owner = null;
        source = null;
        body = null;
        initialNeighbors.Clear();
    }

    private void OnDisable()
    {
        ResetMonitor();
    }
}
