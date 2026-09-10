using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]

public class PlayerStats : ScriptableObject
{
    [Header("Basic Settings")]
    public int maxHealth;
    public float walkingSpeed;
    public float runningSpeed;
    public float maxSpeed;
    public float dashCooldown;

    [Header("Jump Settings")]
    public float minJumpForce;
    public float maxJumpForce;
    public float chargeSpeed;
    public float chargeSlowMultiplier;

    [Header("Falling With Hammer Settings")]
    public float forwardMultiplier; 
    public float downwardMultiplier; 
    public float slamForce;
    public float minDistWGround;

    [Header("Basic Attack Settings")]
    public float basicMaxDamage;
    public float basicHitStopDuration;
    public float speedReductor;
    public float basicAttackRadius;
    public float basicAttackShakeDuration;
    public float basicAttackShakeMagnitude;
    public float basicAttackKickPitch;
    public float basicAttackKickYaw;

    [Header("Second Attack Settings")]
    public float secondMaxDamage;
    public float secondHitStopDuration;
    public float secondAttackRadius;
    public float secondAttackShakeDuration;
    public float secondAttackShakeMagnitude;
    public float secondAttackKickPitch;
    public float secondAttackKickYaw;

    [Header("Charged Attack Settings")]
    public float chargedMaxDamage;
    public float chargedHitStopDuration;
    public float chargedAttackRadius;
    public float chargedAttackShakeDuration;
    public float chargedAttackShakeMagnitude;
    public float chargedAttackKickPitch;
    public float chargedAttackKickYaw;

    [Header("Gravity")]
    public float extraGravityForce;

    [Header("Sens")]
    public float mouseSens;


}
