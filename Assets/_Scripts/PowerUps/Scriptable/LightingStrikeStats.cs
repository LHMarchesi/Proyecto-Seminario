using UnityEngine;

[CreateAssetMenu(fileName = "LightingStrike", menuName = "PowerUpStats/LightingStrike")]

public class LightingStrikeStats : ScriptableObject
{
    public Sprite lightningIconSprite;
    public GameObject lightningEffectPrefab;
    public float cooldown;
    public float additionalDamage;
}
