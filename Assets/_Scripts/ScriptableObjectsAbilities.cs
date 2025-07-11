using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableObjectsAbilities : ScriptableObject
{
    public Sprite lootSprite;
    public string abilityName;
    public int damage;
    public int health;
    public int speed;
    public int dropChance;

    public ScriptableObjectsAbilities(string abilityName, int dropChance)
    {
        this.abilityName = abilityName;
        this.dropChance = dropChance;
    }
}
