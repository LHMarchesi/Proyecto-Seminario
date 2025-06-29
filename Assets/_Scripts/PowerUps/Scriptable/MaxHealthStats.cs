using UnityEngine;

[CreateAssetMenu(fileName = "MaxHealth", menuName = "PowerUpStats/MaxHealth")]

public class MaxHealthStats : ScriptableObject
{
    public Sprite IconSprite;
    public string description; 
    public float newMaxHealth;
}
