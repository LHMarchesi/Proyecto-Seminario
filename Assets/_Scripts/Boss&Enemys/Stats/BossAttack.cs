using UnityEngine;

public abstract class BossAttackStats : ScriptableObject
{
    public string attackName;
    public float cooldown;
    public float damage;
    public float range;

    private float lastAttackTime = -Mathf.Infinity; 
    
    protected bool CanExecute()
    {
        return Time.time >= lastAttackTime + cooldown;
    }
    protected void MarkExecuted()
    {
        lastAttackTime = Time.time;
    }
    public abstract void Execute(BossEnemy boss, Transform target, HandleAnimations animations);

    public bool IsInRange(Transform boss, Transform target)
    {
        float dist = Vector3.Distance(boss.position, target.position);
        return dist <= range;
    }
}
