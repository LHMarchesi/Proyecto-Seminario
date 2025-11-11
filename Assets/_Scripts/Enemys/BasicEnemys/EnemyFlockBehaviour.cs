using System.Collections.Generic;
using UnityEngine;

public class FlockingBehavior : MonoBehaviour
{
    private EnemySpawner spawner;
    private float cohesionWeight, separationWeight, alignmentWeight, neighborRadius;

    private Rigidbody rb;

    public void Initialize(EnemySpawner spawner, float cohesion, float separation, float alignment, float radius)
    {
        this.spawner = spawner;
        this.cohesionWeight = cohesion;
        this.separationWeight = separation;
        this.alignmentWeight = alignment;
        this.neighborRadius = radius;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (spawner == null) return;

        List<GameObject> neighbors = spawner.GetActiveEnemies();
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        int count = 0;

        foreach (var other in neighbors)
        {
            if (other == gameObject) continue;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                cohesion += other.transform.position;
                alignment += other.GetComponent<Rigidbody>().velocity;
                separation += (transform.position - other.transform.position) / (dist * dist);
                count++;
            }
        }

        if (count > 0)
        {
            cohesion = ((cohesion / count) - transform.position).normalized * cohesionWeight;
            alignment = (alignment / count).normalized * alignmentWeight;
            separation = separation.normalized * separationWeight;
        }

        Vector3 flockForce = cohesion + alignment + separation;
        rb.AddForce(flockForce);
    }
}