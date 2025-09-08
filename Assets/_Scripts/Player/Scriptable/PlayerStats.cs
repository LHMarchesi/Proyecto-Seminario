using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]

public class PlayerStats : ScriptableObject
{
    [Header("Basic Settings")]
    public float maxHealth;
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
    public float maxDamage;

    [Header("Gravity")]
    public float extraGravityForce;

    [Header("Sens")]
    public float mouseSens;


}
