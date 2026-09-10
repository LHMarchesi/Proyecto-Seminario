using System.Collections.Generic;
using System;
using UnityEngine;

public class RunInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private Transform runtimeItemsRoot;

    [Header("Build Items")]
    [SerializeField, Min(1)] private int maxBuildItems = 4;

    [SerializeField] private List<OwnedAbility> buildItems = new List<OwnedAbility>();

    public event Action<OwnedAbility> OnBuildItemAdded;
    public event Action<OwnedAbility> OnBuildItemUpgraded;

    public IReadOnlyList<OwnedAbility> BuildItems => buildItems;
    public int BuildItemCount => buildItems.Count;
    public int MaxBuildItems => maxBuildItems;

    private void Awake()
    {
        if (playerContext == null)
            playerContext = GetComponent<PlayerContext>();

        if (runtimeItemsRoot == null)
            runtimeItemsRoot = transform;
    }

    public bool HasFreeSlot()
    {
        return buildItems.Count < maxBuildItems;
    }

    public OwnedAbility GetOwnedAbility(AbilityEntry ability)
    {
        if (ability == null)
            return null;

        return GetOwnedAbility(ability.Id);
    }

    public OwnedAbility GetOwnedAbility(string abilityId)
    {
        if (string.IsNullOrWhiteSpace(abilityId))
            return null;

        return buildItems.Find(item =>
            item != null &&
            item.Data != null &&
            string.Equals(item.Data.Id, abilityId, StringComparison.OrdinalIgnoreCase));
    }

    public int GetAbilityLevel(AbilityEntry ability)
    {
        OwnedAbility owned = GetOwnedAbility(ability);
        return owned != null ? owned.Level : 0;
    }

    public int GetAbilityLevel(string abilityId)
    {
        OwnedAbility owned = GetOwnedAbility(abilityId);
        return owned != null ? owned.Level : 0;
    }

    public bool CanOffer(AbilityEntry ability)
    {
        if (ability == null || ability.abilityPrefab == null)
            return false;

        OwnedAbility owned = GetOwnedAbility(ability);

        if (owned != null)
            return owned.Level < ability.MaxLevel;

        return HasFreeSlot();
    }

    public bool AddOrUpgrade(AbilityEntry ability)
    {
        if (ability == null)
            return false;

        OwnedAbility owned = GetOwnedAbility(ability);

        if (owned != null)
            return UpgradeAbility(owned);

        return AddAbility(ability);
    }

    private bool AddAbility(AbilityEntry ability)
    {
        if (!HasFreeSlot())
        {
            Debug.LogWarning($"RunInventory: no hay slots libres para {ability.abilityName}.");
            return false;
        }

        if (ability.abilityPrefab == null)
        {
            Debug.LogError($"RunInventory: {ability.abilityName} no tiene abilityPrefab asignado.");
            return false;
        }

        GameObject instance = Instantiate(ability.abilityPrefab, runtimeItemsRoot);
        instance.name = $"Runtime_{ability.Id}";
        instance.transform.localPosition = Vector3.zero;

        BasePowerUp powerUp = instance.GetComponent<BasePowerUp>();
        if (powerUp == null)
            powerUp = instance.GetComponentInChildren<BasePowerUp>(true);

        if (powerUp == null)
        {
            Debug.LogError($"RunInventory: el prefab {ability.abilityPrefab.name} no contiene BasePowerUp.");
            Destroy(instance);
            return false;
        }

        powerUp.AcquireForRun(playerContext);

        OwnedAbility newAbility = new OwnedAbility(ability, 1, powerUp);
        buildItems.Add(newAbility);

        if (UIManager.Instance != null)
            UIManager.Instance.RegisterHability(ability.Id, ability.icon, newAbility.Level);

        OnBuildItemAdded?.Invoke(newAbility);
        return true;
    }

    private bool UpgradeAbility(OwnedAbility owned)
    {
        if (owned == null || owned.Data == null)
            return false;

        if (owned.Level >= owned.Data.MaxLevel)
            return false;

        owned.IncreaseLevel();

        if (owned.RuntimePowerUp != null)
            owned.RuntimePowerUp.UpgradePowerUp();

        if (UIManager.Instance != null)
            UIManager.Instance.SetHabilityLevel(owned.Data.Id, owned.Level);

        OnBuildItemUpgraded?.Invoke(owned);
        return true;
    }

    public bool AreAllOwnedItemsMaxed()
    {
        if (buildItems.Count == 0)
            return false;

        foreach (OwnedAbility item in buildItems)
        {
            if (item == null || item.Data == null)
                continue;

            if (item.Level < item.Data.MaxLevel)
                return false;
        }

        return true;
    }
}
