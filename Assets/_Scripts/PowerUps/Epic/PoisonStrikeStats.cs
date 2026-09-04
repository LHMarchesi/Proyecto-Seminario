using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PoisonStrikeStats", menuName = "PowerUps/PoisonStrikeStats")]
public class PoisonStrikeStats : ScriptableObject
{
    [Header("Visual")]
    public Sprite IconSprite;
    public GameObject poisonVFXPrefab;

    [Header("Poison")]
    public float poisonDamagePerSecond = 5f;
    public float poisonDuration = 3f;
    public float tickInterval = 0.5f;

    [Header("Level 5 - Spread")]
    public float spreadRadius = 4f;
    public int maxSpreadTargets = 4;
    public LayerMask enemyLayer;

    // Se mantienen por compatibilidad con el PoisonPowerUp viejo.
    [Header("Legacy")]
    public float cooldown = 1.5f;
    public float upgradeDamageIncrease = 2f;
    public float upgradeDurationIncrease = 1f;
}
