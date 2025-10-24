using System.Collections.Generic;
using UnityEngine;

public class EnemyFlockBehaviour : MonoBehaviour
{
    [Header("Flocking Settings")]
    public float speed = 5f;
    public float neighborRadius = 10f;
    public float separationDistance = 3f;

    [Range(0f, 5f)] public float alignmentWeight = 1f;
    [Range(0f, 5f)] public float cohesionWeight = 1f;
    [Range(0f, 5f)] public float separationWeight = 1f;

    private Rigidbody rb;
    private Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        FlockManager.Instance.RegisterAgent(this);
    }

    private void OnDestroy()
    {
        if (FlockManager.Instance != null)
            FlockManager.Instance.UnregisterAgent(this);
    }

    private void FixedUpdate()
    {
        List<EnemyFlockBehaviour> nearbyAgents = FlockManager.Instance.GetNearbyAgents(this);
        Vector3 moveDirection = CalculateFlocking(nearbyAgents);
        rb.velocity = moveDirection * speed;
    }

    private Vector3 CalculateFlocking(List<EnemyFlockBehaviour> agents)
    {
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;

        int neighborCount = 0;

        foreach (var other in agents)
        {
            if (other == this || other == null) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance > neighborRadius) continue;

            alignment += other.transform.forward;
            cohesion += other.transform.position;

            if (distance < separationDistance)
                separation += (transform.position - other.transform.position) / distance;

            neighborCount++;
        }

        if (neighborCount > 0)
        {
            alignment = (alignment / neighborCount).normalized * alignmentWeight;
            cohesion = ((cohesion / neighborCount) - transform.position).normalized * cohesionWeight;
            separation = separation.normalized * separationWeight;
        }

        // Dirigirse también al jugador (si existe)
        Vector3 targetDir = Vector3.zero;
        if (player != null)
            targetDir = (player.position - transform.position).normalized * 0.5f;

        Vector3 finalDir = (alignment + cohesion + separation + targetDir).normalized;

        if (finalDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(finalDir);

        return finalDir;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f); // Azul translúcido
        Gizmos.DrawWireSphere(transform.position, neighborRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // Naranja translúcido
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
#endif
}
