using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AuraStrikeStats", menuName = "PowerUps/AuraStrikeStats")]
public class AuraStrikeStats : ScriptableObject
{
    public Sprite IconSprite;
    public float range = 5f;
    public float damagePerTick = 10f;
    public float tickInterval = 1f;
    public LayerMask enemyLayer;

    // Parámetros de mejora opcionales
    public float upgradeDamageIncrease = 2f;
    public float upgradeRangeIncrease = 0.5f;
}
