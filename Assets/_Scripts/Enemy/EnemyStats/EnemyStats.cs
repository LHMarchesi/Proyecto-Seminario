using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Stats")]
public class EnemyStats : ScriptableObject
{
    public float maxHealth;
    public float moveSpeed; 
    public float attackDamage;
    [Tooltip("Attacks per second")]
    public float attackSpeed;
    public float attackRange;
    public float detectionRange;
}


