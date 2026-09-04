using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Experience/Experience Table")]
public class ExperienceTable : ScriptableObject
{
    [Tooltip("Umbrales acumulativos de XP. Ej.: 10, 30, 60 significa Lv1 a 10 XP, Lv2 a 30 XP total, etc.")]
    public int[] xpNeededPerLevel;

    [Header("Escalado después de la tabla")]
    [SerializeField, Min(1)] private int fallbackFirstLevelXP = 10;
    [SerializeField, Min(1f)] private float infiniteGrowthMultiplier = 1.15f;

    public float GetCumulativeXPThreshold(int levelIndex)
    {
        levelIndex = Mathf.Max(0, levelIndex);

        if (xpNeededPerLevel == null || xpNeededPerLevel.Length == 0)
        {
            return GetFallbackThreshold(levelIndex);
        }

        if (levelIndex < xpNeededPerLevel.Length)
            return Mathf.Max(1, xpNeededPerLevel[levelIndex]);

        float total = xpNeededPerLevel[xpNeededPerLevel.Length - 1];

        float previousThreshold = xpNeededPerLevel.Length >= 2
            ? xpNeededPerLevel[xpNeededPerLevel.Length - 2]
            : 0f;

        float lastCost = Mathf.Max(1f, total - previousThreshold);
        int extraLevels = levelIndex - (xpNeededPerLevel.Length - 1);

        for (int i = 0; i < extraLevels; i++)
        {
            lastCost = Mathf.Ceil(lastCost * infiniteGrowthMultiplier);
            total += lastCost;
        }

        return total;
    }

    private float GetFallbackThreshold(int levelIndex)
    {
        float total = 0f;
        float cost = fallbackFirstLevelXP;

        for (int i = 0; i <= levelIndex; i++)
        {
            total += cost;
            cost = Mathf.Ceil(cost * infiniteGrowthMultiplier);
        }

        return total;
    }
}

[Serializable]
public class OwnedAbility
{
    [SerializeField] private AbilityEntry data;
    [SerializeField] private int level;
    [SerializeField] private BasePowerUp runtimePowerUp;

    public AbilityEntry Data => data;
    public int Level => level;
    public BasePowerUp RuntimePowerUp => runtimePowerUp;

    public OwnedAbility(AbilityEntry data, int level, BasePowerUp runtimePowerUp)
    {
        this.data = data;
        this.level = level;
        this.runtimePowerUp = runtimePowerUp;
    }

    public void IncreaseLevel()
    {
        level++;
    }
}
