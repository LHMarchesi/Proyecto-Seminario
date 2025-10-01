using UnityEngine;

[CreateAssetMenu(fileName = "TeleportPowerUp", menuName = "PowerUpStats/Teleport")]
public class TeleportPowerUpStats : ScriptableObject
{
    public Sprite IconSprite;
    public GameObject EffectPrefab;
    public float cooldown;
    public float additionalDamage;
}
