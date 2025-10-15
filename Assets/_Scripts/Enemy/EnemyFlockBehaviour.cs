using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlockBehaviour : MonoBehaviour
{
    [Header("Flocking Settings")]
    public float neighborRadius = 6f;
    public float separationRadius = 2f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Flocking Weights")]
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float separationWeight = 1.5f;
    public float targetWeight = 1f;

    [Header("Detección y Patrulla")]
    public float detectionRange = 10f;
    public float patrolRadius = 15f;
    public float patrolPointChangeTime = 5f;

    [Header("Target (asignado automáticamente por tag 'Player')")]
    public Transform target;

    private List<EnemyFlockBehaviour> allEnemies;
    private Vector3 currentPatrolPoint;
    private float patrolTimer;
    private bool isChasing = false;

    protected virtual void Start()
    {
        // Buscar automáticamente al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            Debug.LogWarning($"{name} no encontró un objeto con tag 'Player'.");

        allEnemies = new List<EnemyFlockBehaviour>(FindObjectsOfType<EnemyFlockBehaviour>());

        SetNewPatrolPoint();
    }

    protected virtual void Update()
    {
        // Si aún no está persiguiendo, comprobar si el jugador entra en rango
        if (target != null && !isChasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= detectionRange)
            {
                StartChase(); // 🔹 activa el modo persecución y alerta al grupo
            }
        }

        Vector3 moveDir = isChasing ? ComputeFlockDirection() : ComputePatrolDirection();
        MoveTowards(moveDir);
    }

    // --- 🔹 ALERTA GRUPAL ---
    private void StartChase()
    {
        isChasing = true;

        // 🔹 Avisar a todos los demás enemigos del grupo
        foreach (var other in allEnemies)
        {
            if (other != null && !other.isChasing)
                other.isChasing = true;
        }
    }

    // --- MOVIMIENTO ---
    protected void MoveTowards(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }

    // --- FLOCKING ---
    private Vector3 ComputeFlockDirection()
    {
        Vector3 alignment = ComputeAlignment() * alignmentWeight;
        Vector3 cohesion = ComputeCohesion() * cohesionWeight;
        Vector3 separation = ComputeSeparation() * separationWeight;
        Vector3 targetDir = ComputeTargetDirection() * targetWeight;

        Vector3 combined = alignment + cohesion + separation + targetDir;
        return combined.normalized;
    }

    private Vector3 ComputeTargetDirection()
    {
        if (target == null) return Vector3.zero;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        return dir;
    }

    private Vector3 ComputeAlignment()
    {
        Vector3 avgDir = Vector3.zero;
        int count = 0;

        foreach (var other in allEnemies)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                avgDir += other.transform.forward;
                count++;
            }
        }

        if (count > 0)
            avgDir /= count;

        return avgDir.normalized;
    }

    private Vector3 ComputeCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var other in allEnemies)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                center += other.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            center /= count;
            Vector3 dirToCenter = (center - transform.position).normalized;
            return dirToCenter;
        }

        return Vector3.zero;
    }

    private Vector3 ComputeSeparation()
    {
        Vector3 avoid = Vector3.zero;
        int count = 0;

        foreach (var other in allEnemies)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < separationRadius && dist > 0)
            {
                avoid += (transform.position - other.transform.position).normalized / dist;
                count++;
            }
        }

        if (count > 0)
            avoid /= count;

        return avoid.normalized;
    }

    // --- PATRULLA ALEATORIA ---
    private Vector3 ComputePatrolDirection()
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolPointChangeTime || Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
        {
            SetNewPatrolPoint();
        }

        Vector3 dir = (currentPatrolPoint - transform.position).normalized;
        dir.y = 0;
        return dir;
    }

    private void SetNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentPatrolPoint = new Vector3(randomCircle.x + transform.position.x, transform.position.y, randomCircle.y + transform.position.z);
        patrolTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
