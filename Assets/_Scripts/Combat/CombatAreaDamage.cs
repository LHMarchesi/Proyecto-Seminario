using System.Collections.Generic;
using UnityEngine;

// Consultas de combate: nunca usa triggers de aggro, pilares o pickups.
public static class CombatAreaDamage
{
    public static List<BaseEnemy> FindTargets(Vector3 center, float radius,
        LayerMask enemyLayer, int maxTargets = 64, BaseEnemy excluded = null)
    {
        var results = new List<BaseEnemy>();
        var unique = new HashSet<BaseEnemy>();
        if (radius <= 0f || maxTargets <= 0) return results;

        Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer,
            QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null || enemy == excluded || enemy.IsDead() ||
                !enemy.gameObject.activeInHierarchy || !unique.Add(enemy)) continue;
            results.Add(enemy);
        }
        results.Sort((a, b) =>
            (a.transform.position - center).sqrMagnitude.CompareTo(
                (b.transform.position - center).sqrMagnitude));
        if (results.Count > maxTargets)
            results.RemoveRange(maxTargets, results.Count - maxTargets);
        return results;
    }

    public static int DealDamage(Vector3 center, float radius, LayerMask enemyLayer,
        float damage, DamageFeedbackType type, float force = 0f,
        float upwardForce = 0f, int maxTargets = 64, BaseEnemy excluded = null)
    {
        if (damage <= 0f && force <= 0f) return 0;
        List<BaseEnemy> targets = FindTargets(center, radius, enemyLayer, maxTargets, excluded);
        int count = 0;
        foreach (BaseEnemy enemy in targets)
        {
            if (enemy == null || enemy.IsDead()) continue;
            Vector3 direction = enemy.transform.position - center;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector3.forward;
            if (damage > 0f) enemy.TakeEffectDamage(damage, type);
            if (force > 0f && !enemy.IsDead())
                enemy.ApplyExternalKnockback(direction.normalized, force, upwardForce);
            count++;
        }
        return count;
    }
}
