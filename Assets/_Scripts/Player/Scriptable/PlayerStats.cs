using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]

public class PlayerStats : ScriptableObject
{
    [Header("Basic Settings")]
    public int maxHealth;
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
    public float speedReductor;
    public float basicAttackRadius;
    public float basicAttackShakeDuration;
    public float basicAttackShakeMagnitude;

    [Header("Charged Attack Settings")]
    public float chargedMaxDamage;
    public float chargedAttackRadius;
    public float chargedAttackShakeDuration;
    public float chargedAttackShakeMagnitude;

    [Header("Gravity")]
    public float extraGravityForce;

    [Header("Sens")]
    public float mouseSens;


}
