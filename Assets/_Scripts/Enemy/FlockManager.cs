using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [SerializeField] private List<BaseEnemy> enemies = new List<BaseEnemy>();
    public float neighborRadius;
    public float separationRadius;
    public float cohesionWeight;
    public float separationWeight;
    public float targetWeight;
    public float alignmentWeight;

    public void Register(BaseEnemy enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    private void Awake()
    {
        TryRegistChildren();
    }

    private void TryRegistChildren()
    {
        BaseEnemy[] childrenEnemies = GetComponentsInChildren<BaseEnemy>();
        foreach (var enemy in childrenEnemies)
        {
            Register(enemy);
        }
    }

    public void Unregister(BaseEnemy enemy)
    {
        enemies.Remove(enemy);
        if(enemies.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    public Vector3 ComputeFlockDirection(BaseEnemy self, Transform target,
        float neighborRadius, float separationRadius,
        float alignmentWeight, float cohesionWeight,
        float separationWeight, float targetWeight)
    {
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (var other in enemies)
        {
            if (other == null || other == self) continue;
            float dist = Vector3.Distance(self.transform.position, other.transform.position);
            if (dist < neighborRadius)
            {
                alignment += other.transform.forward;
                cohesion += other.transform.position;
                if (dist < separationRadius)
                    separation += (self.transform.position - other.transform.position).normalized / dist;

                count++;
            }
        }

        if (count > 0)
        {
            alignment /= count;
            cohesion = ((cohesion / count) - self.transform.position).normalized;
            separation /= count;
        }

        Vector3 targetDir = (target != null) ? (target.position - self.transform.position).normalized : Vector3.zero;

        Vector3 combined = alignment * alignmentWeight +
                           cohesion * cohesionWeight +
                           separation * separationWeight +
                           targetDir * targetWeight;

        combined.y = 0;
        return combined.normalized;
    }
}
