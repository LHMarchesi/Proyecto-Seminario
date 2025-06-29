using UnityEngine;

[CreateAssetMenu(fileName = "ExplodeStrike", menuName = "PowerUpStats/ExplodeStrike")]

public class ExplodeStrikeStats : ScriptableObject
{
    public Sprite IconSprite;
    public GameObject EffectPrefab; // Prefab (Efecto)
    public string description; // 
    public float explosionRange;
    public float explosionForce;
    public float explosionDamage;
    public float cooldown;
    public LayerMask enemyLayer;

}
