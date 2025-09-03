using UnityEngine;
[CreateAssetMenu(fileName = "MeleeBossAttack", menuName = "BossAttacks/MeleeAttack")]
public class MeleeBossAttack : BossAttackStats
{
    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        if (animations != null && !string.IsNullOrEmpty(animationName))
            animations.ChangeAnimationState(animationName);

        // Preparar daño y ejecutar ataque
        boss.DoAttack("Melee",damage, attackDelay, attackDuration, knockbackHorizontal, knockbackVertical);

        Debug.Log($"Executed {attackName} on {boss.name}");
    }
}


