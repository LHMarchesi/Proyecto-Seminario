using UnityEngine;

public class FollowTargetPowerUp : BasePowerUp
{
    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnMjolnirThrow += MjolnirFollowEnemy;
    }

    void MjolnirFollowEnemy()
    {
        Transform closestEnemy = FindClosestEnemy();

    }

    Transform FindClosestEnemy()
    {
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy")) // Asegurate de usar el tag "Enemy"
        {
            float distance = Vector3.Distance(playerContext.transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }
}
