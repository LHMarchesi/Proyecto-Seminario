using UnityEngine;

[CreateAssetMenu(fileName = "MaxJumpForce", menuName = "PowerUpStats/MaxJumpForce")]

public class MaxJumpForceStats : ScriptableObject
{
    public Sprite IconSprite;
    public string description;
    public float newMaxJumpForce;
}
