using System.Collections.Generic;
using UnityEngine;

public static class CombatTargeting
{
    public static List<BaseEnemy> FindClosestEnemies(
        Vector3 origin,
        float radius,
        LayerMask enemyLayer,
        int maxTargets,
        BaseEnemy excludedEnemy = null)
    {
        List<BaseEnemy> results = new List<BaseEnemy>();
        HashSet<BaseEnemy> uniqueEnemies = new HashSet<BaseEnemy>();

        if (maxTargets <= 0)
            return results;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            radius,
            enemyLayer,
            QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();

            if (enemy == null || enemy == excludedEnemy || enemy.IsDead())
                continue;

            if (uniqueEnemies.Add(enemy))
                results.Add(enemy);
        }

        results.Sort((a, b) =>
        {
            float distanceA = (a.transform.position - origin).sqrMagnitude;
            float distanceB = (b.transform.position - origin).sqrMagnitude;
            return distanceA.CompareTo(distanceB);
        });

        if (results.Count > maxTargets)
            results.RemoveRange(maxTargets, results.Count - maxTargets);

        return results;
    }
}
