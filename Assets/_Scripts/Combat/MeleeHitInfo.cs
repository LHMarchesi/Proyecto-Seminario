using UnityEngine;

public struct MeleeHitInfo
{
    public MeleeAttackType AttackType;
    public Collider Collider;
    public BaseEnemy Enemy;
    public Vector3 HitPoint;
    public Vector3 HitDirection;
    public float Damage;
    public float KnockbackForce;

    public MeleeHitInfo(
        MeleeAttackType attackType,
        Collider collider,
        BaseEnemy enemy,
        Vector3 hitPoint,
        Vector3 hitDirection,
        float damage,
        float knockbackForce)
    {
        AttackType = attackType;
        Collider = collider;
        Enemy = enemy;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        Damage = damage;
        KnockbackForce = knockbackForce;
    }
}
