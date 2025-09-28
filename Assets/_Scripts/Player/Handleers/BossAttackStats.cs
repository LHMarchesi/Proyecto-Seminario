using UnityEngine;

public abstract class BossAttackStats : ScriptableObject
{
    public string attackName;
    public string animationName;
    public float attackDelay;
    public float attackDuration;
    public float cooldown;
    public float range;

    public abstract void Execute(BossEnemy boss, Transform target, HandleAnimations animations);

    public bool IsInRange(Transform boss, Transform target)
    {
        float dist = Vector3.Distance(boss.position, target.position);
        return dist <= range;
    }
}
