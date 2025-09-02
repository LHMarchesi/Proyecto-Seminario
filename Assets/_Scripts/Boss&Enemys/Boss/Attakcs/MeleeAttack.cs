using UnityEngine;

[CreateAssetMenu(menuName = "BossAttacks/MeleeAttack")]
public class MeleeAttack : BossAttackStats
{
    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        if (!CanExecute()) return;

        float dist = Vector3.Distance(boss.transform.position, target.position);
        if (dist <= range)
        {
            boss.PrepareAttack(damage);
            animations.ChangeAnimationState("MeleeAttack");
            MarkExecuted();
        }
    }
}