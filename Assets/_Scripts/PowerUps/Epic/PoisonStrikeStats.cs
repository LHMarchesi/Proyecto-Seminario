using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PoisonStrikeStats", menuName = "PowerUps/PoisonStrikeStats")]
public class PoisonStrikeStats : ScriptableObject
{
    public Sprite IconSprite;
    public float poisonDamagePerSecond = 5f;
    public float poisonDuration = 3f;
    public float cooldown = 1.5f;

    // Parámetros de mejora opcionales
    public float upgradeDamageIncrease = 2f;
    public float upgradeDurationIncrease = 1f;
}
