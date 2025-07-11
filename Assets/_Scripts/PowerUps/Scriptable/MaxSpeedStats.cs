using UnityEngine;

[CreateAssetMenu(fileName = "MaxSpeed", menuName = "PowerUpStats/MaxSpeed")]

public class MaxSpeedStats : ScriptableObject
{
    public Sprite IconSprite;
    public string description; 
    public float newMaxSpeed;
}
