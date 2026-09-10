using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private RunInventory runInventory;

    [Header("Habilidades disponibles")]
    public List<AbilityEntry> availableAbilities = new List<AbilityEntry>();

    [Header("Tabla de Experiencia")]
    [SerializeField] private ExperienceTable experienceTable;

    [Header("Interface de experiencia")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private SliderPassValue sliderPass;

    [Header("Panel de Level Up")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform abilityButtonContainer;
    [SerializeField] private GameObject abilityButtonPrefab;
    [SerializeField, Min(1)] private int optionsPerLevel = 2;

    [Header("Fallback de curación")]
    [SerializeField] private Sprite healIcon;
    [SerializeField] private string healName = "Recuperar vida";
    [SerializeField, TextArea] private string healDescription = "Recupera un porcentaje de la vida máxima.";
    [SerializeField, Range(0.01f, 1f)] private float healPercentOfMaxHealth = 0.25f;

    private int currentLevel;
    private float totalExperience;
    private float previousLevelsExperience;
    private float nextLevelsExperience;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();

    private int pendingLevelUps;
    private bool selectionInProgress;
    private bool optionSelected;

    public delegate void OnLevelUpEvent();
    public event OnLevelUpEvent OnLevelUp;

    public int CurrentLevel => currentLevel;
    public float TotalExperience => totalExperience;
    public RunInventory Inventory => runInventory;

    private void Awake()
    {
        if (playerContext == null)
            playerContext = GetComponent<PlayerContext>();

        if (runInventory == null)
            runInventory = GetComponent<RunInventory>();

        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        UpdateInterface();
    }

    public void AddExperience(float amount)
    {
        if (amount <= 0f)
            return;

        totalExperience += amount;
        CheckForLevelUp();
        UpdateInterface();
    }

    private void CheckForLevelUp()
    {
        if (experienceTable == null)
        {
            Debug.LogError("ExperienceManager: falta ExperienceTable.");
            return;
        }

        float nextThreshold = experienceTable.GetCumulativeXPThreshold(currentLevel);

        while (totalExperience >= nextThreshold)
        {
            currentLevel++;
            pendingLevelUps++;
            nextThreshold = experienceTable.GetCumulativeXPThreshold(currentLevel);
        }

        if (!selectionInProgress && pendingLevelUps > 0)
            StartCoroutine(ProcessPendingLevelUps());
    }

    private IEnumerator ProcessPendingLevelUps()
    {
        selectionInProgress = true;
        PauseGameplayForLevelUp();

        while (pendingLevelUps > 0)
        {
            yield return OpenLevelUpPanel();
            pendingLevelUps--;
        }

        ResumeGameplayAfterLevelUp();
        selectionInProgress = false;
    }

    private IEnumerator OpenLevelUpPanel()
    {
        optionSelected = false;
        OnLevelUp?.Invoke();

        ClearSpawnedButtons();

        if (panel != null)
            panel.SetActive(true);

        List<AbilityEntry> options = GetRandomAbilityOptions(optionsPerLevel);

        foreach (AbilityEntry ability in options)
            SpawnAbilityButton(ability);

        // Si sólo queda una mejora válida, la segunda opción es vida.
        // Si ya no queda ninguna mejora, se muestra únicamente vida.
        if (options.Count < optionsPerLevel)
            SpawnHealButton();

        UpdateInterface();

        while (!optionSelected)
            yield return null;

        if (panel != null)
            panel.SetActive(false);

        ClearSpawnedButtons();
    }

    private void SpawnAbilityButton(AbilityEntry ability)
    {
        if (abilityButtonPrefab == null || abilityButtonContainer == null)
            return;

        GameObject buttonGO = Instantiate(abilityButtonPrefab, abilityButtonContainer);
        spawnedButtons.Add(buttonGO);

        AbilityButtonUI buttonUI = buttonGO.GetComponent<AbilityButtonUI>();
        if (buttonUI != null)
            buttonUI.Setup(ability, this);
    }

    private void SpawnHealButton()
    {
        if (abilityButtonPrefab == null || abilityButtonContainer == null)
            return;

        GameObject buttonGO = Instantiate(abilityButtonPrefab, abilityButtonContainer);
        spawnedButtons.Add(buttonGO);

        AbilityButtonUI buttonUI = buttonGO.GetComponent<AbilityButtonUI>();
        if (buttonUI != null)
            buttonUI.SetupHeal(this, healIcon, healName, healDescription, healPercentOfMaxHealth);
    }

    public void ApplySelectedAbility(AbilityEntry selectedAbility)
    {
        if (optionSelected || selectedAbility == null || runInventory == null)
            return;

        if (!runInventory.AddOrUpgrade(selectedAbility))
        {
            Debug.LogWarning($"No se pudo agregar/mejorar {selectedAbility.abilityName}.");
            return;
        }

        CompleteSelection();
    }

    public void ApplyHealOption(float healPercent)
    {
        if (optionSelected || playerContext == null || playerContext.PlayerController == null)
            return;

        int maxHealth = playerContext.PlayerController.MaxHealth;
        int healAmount = Mathf.Max(1, Mathf.CeilToInt(maxHealth * healPercent));
        playerContext.PlayerController.AddHealth(healAmount);

        CompleteSelection();
    }

    private void CompleteSelection()
    {
        optionSelected = true;
    }

    public int GetAbilityLevel(AbilityEntry ability)
    {
        return runInventory != null ? runInventory.GetAbilityLevel(ability) : 0;
    }

    public int GetAbilityLevel(string abilityId)
    {
        return runInventory != null ? runInventory.GetAbilityLevel(abilityId) : 0;
    }

    private List<AbilityEntry> GetRandomAbilityOptions(int count)
    {
        List<AbilityEntry> candidates = new List<AbilityEntry>();

        foreach (AbilityEntry ability in availableAbilities)
        {
            if (ability == null)
                continue;

            if (runInventory != null && runInventory.CanOffer(ability))
                candidates.Add(ability);
        }

        List<AbilityEntry> result = new List<AbilityEntry>();

        while (result.Count < count && candidates.Count > 0)
        {
            AbilityEntry chosen = PickWeightedAbility(candidates);
            if (chosen == null)
                break;

            result.Add(chosen);
            candidates.Remove(chosen);
        }

        return result;
    }

    private AbilityEntry PickWeightedAbility(List<AbilityEntry> candidates)
    {
        float totalWeight = 0f;

        foreach (AbilityEntry ability in candidates)
            totalWeight += Mathf.Max(0.01f, ability.dropChance);

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        foreach (AbilityEntry ability in candidates)
        {
            roll -= Mathf.Max(0.01f, ability.dropChance);
            if (roll <= 0f)
                return ability;
        }

        return candidates.Count > 0 ? candidates[candidates.Count - 1] : null;
    }

    private void UpdateInterface()
    {
        if (experienceTable == null)
            return;

        previousLevelsExperience = currentLevel <= 0
            ? 0f
            : experienceTable.GetCumulativeXPThreshold(currentLevel - 1);

        nextLevelsExperience = experienceTable.GetCumulativeXPThreshold(currentLevel);

        float currentXP = Mathf.Max(0f, totalExperience - previousLevelsExperience);
        float neededXP = Mathf.Max(1f, nextLevelsExperience - previousLevelsExperience);

        if (sliderPass != null)
        {
            sliderPass.SetMax(neededXP);
            sliderPass.ChangeValue(Mathf.Min(currentXP, neededXP));
        }

        if (levelText != null)
            levelText.text = $"Nivel {currentLevel}";
    }

    private void PauseGameplayForLevelUp()
    {
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerContext != null && playerContext.HandleInputs != null)
            playerContext.HandleInputs.SetPaused(true);
    }

    private void ResumeGameplayAfterLevelUp()
    {
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerContext != null && playerContext.HandleInputs != null)
            playerContext.HandleInputs.SetPaused(false);
    }

    private void ClearSpawnedButtons()
    {
        foreach (GameObject go in spawnedButtons)
        {
            if (go != null)
                Destroy(go);
        }

        spawnedButtons.Clear();
    }
}

[Serializable]
public class AbilityEntry
{
    [Header("Identidad")]
    public string abilityId;
    public string abilityName;
    [TextArea] public string abilityDescription;
    public GameObject abilityPrefab;
    public Sprite icon;

    [Header("Progresión")]
    [Min(1)] public int maxLevel = 5;
    [Tooltip("Índice 0 = descripción de Lv.1, índice 1 = Lv.2, etc.")]
    [TextArea] public List<string> levelDescriptions = new List<string>();

    [Header("Peso de aparición")]
    [Range(1f, 100f)] public float dropChance = 100f;

    public string Id => string.IsNullOrWhiteSpace(abilityId) ? abilityName : abilityId;
    public int MaxLevel => Mathf.Max(1, maxLevel);

    public string GetDescriptionForLevel(int level)
    {
        int index = level - 1;

        if (levelDescriptions != null && index >= 0 && index < levelDescriptions.Count)
        {
            string description = levelDescriptions[index];
            if (!string.IsNullOrWhiteSpace(description))
                return description;
        }

        return abilityDescription;
    }
}
