using UnityEngine;

[CreateAssetMenu(fileName = "MeleeBossAttack", menuName = "BossAttacks/JumpAttack")]
public class JumpAttack : BossAttackStats
{
    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        if (animations != null && !string.IsNullOrEmpty(animationName))
            animations.ChangeAnimationState(animationName);

        // Preparar daño y ejecutar ataque
        boss.DoAttack("Area", damage, attackDelay, attackDuration, knockbackHorizontal, knockbackVertical);

        Debug.Log($"Executed {attackName} on {boss.name}");
    }
}


