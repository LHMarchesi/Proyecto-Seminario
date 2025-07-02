using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]

public class PlayerStats : ScriptableObject
{
    public float maxHealth;
    public float walkingSpeed;
    public float runningSpeed;
    public float maxSpeed;
    public float jumpForce;
    public float maxDamage;
    public float mouseSens;
    public float extraGravityForce; 
    public float dashCooldown;
}
