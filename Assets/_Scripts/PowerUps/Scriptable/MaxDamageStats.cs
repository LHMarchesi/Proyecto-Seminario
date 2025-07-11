using UnityEngine;

[CreateAssetMenu(fileName = "MaxDamage", menuName = "PowerUpStats/MaxDamage")]

public class MaxDamageStats : ScriptableObject
{
    public Sprite IconSprite;
    public string description;
    public float newMaxDamage;
}