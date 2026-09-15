using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Level Up - Reveal")]
    [Tooltip("Escala inicial de cada opcion antes de aparecer.")]
    [SerializeField, Range(0f, 1f)] private float optionRevealStartScale = 0.75f;

    [Tooltip("Duracion del scale/fade de cada opcion.")]
    [SerializeField, Min(0f)] private float optionRevealDuration = 0.18f;

    [Tooltip("Tiempo entre la aparicion de una opcion y la siguiente.")]
    [SerializeField, Min(0f)] private float optionRevealStagger = 0.08f;

    [Tooltip("Delay despues de mostrar todas las opciones antes de habilitar la seleccion.")]
    [SerializeField, Min(0f)] private float selectionUnlockDelay = 0.15f;

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
    private bool selectionUnlocked;

    public delegate void OnLevelUpEvent();
    public event OnLevelUpEvent OnLevelUp;

    public int CurrentLevel => currentLevel;
    public float TotalExperience => totalExperience;
    public RunInventory Inventory => runInventory;

    private void Awake()
    {
        if (runInventory == null)
            runInventory = GetComponent<RunInventory>();

        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (playerContext == null)
            playerContext = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerContext>();
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
        selectionUnlocked = false;

        OnLevelUp?.Invoke();

        ClearSpawnedButtons();

        if (panel != null)
            panel.SetActive(true);

        List<AbilityEntry> options =
            GetRandomAbilityOptions(optionsPerLevel);

        foreach (AbilityEntry ability in options)
            SpawnAbilityButton(ability);

        // Si sólo queda una mejora válida, la segunda opción es vida.
        // Si ya no queda ninguna mejora, se muestra únicamente vida.
        if (options.Count < optionsPerLevel)
            SpawnHealButton();

        UpdateInterface();

        // Importante:
        // El gameplay ya está pausado con Time.timeScale = 0,
        // por eso todo el reveal usa tiempo NO escalado.
        Canvas.ForceUpdateCanvases();

        PrepareButtonsForReveal();

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            GameObject buttonGO =
                spawnedButtons[i];

            if (buttonGO == null)
                continue;

            yield return StartCoroutine(
                RevealButton(buttonGO));

            if (optionRevealStagger > 0f &&
                i < spawnedButtons.Count - 1)
            {
                yield return new WaitForSecondsRealtime(
                    optionRevealStagger);
            }
        }

        // Pequeña protección contra clicks instantáneos.
        if (selectionUnlockDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                selectionUnlockDelay);
        }

        selectionUnlocked = true;
        SetButtonsSelectable(true);

        while (!optionSelected)
            yield return null;

        selectionUnlocked = false;
        SetButtonsSelectable(false);

        if (panel != null)
            panel.SetActive(false);

        ClearSpawnedButtons();
    }

    private void PrepareButtonsForReveal()
    {
        foreach (GameObject buttonGO in spawnedButtons)
        {
            if (buttonGO == null)
                continue;

            CanvasGroup group =
                buttonGO.GetComponent<CanvasGroup>();

            if (group == null)
                group = buttonGO.AddComponent<CanvasGroup>();

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Button button =
                buttonGO.GetComponentInChildren<Button>(true);

            if (button != null)
                button.interactable = false;

            Vector3 baseScale =
                buttonGO.transform.localScale;

            buttonGO.transform.localScale =
                baseScale *
                Mathf.Clamp01(optionRevealStartScale);
        }
    }

    private IEnumerator RevealButton(
        GameObject buttonGO)
    {
        if (buttonGO == null)
            yield break;

        CanvasGroup group =
            buttonGO.GetComponent<CanvasGroup>();

        if (group == null)
            group = buttonGO.AddComponent<CanvasGroup>();

        float startScale =
            Mathf.Clamp01(optionRevealStartScale);

        Vector3 targetScale =
            buttonGO.transform.localScale;

        // Recover the original scale from the prepared scale.
        if (startScale > 0.001f)
        {
            targetScale /=
                startScale;
        }
        else
        {
            // En caso de Start Scale = 0, usamos escala 1 como destino.
            targetScale =
                Vector3.one;
        }

        Vector3 initialScale =
            targetScale * startScale;

        buttonGO.transform.localScale =
            initialScale;

        if (optionRevealDuration <= 0f)
        {
            buttonGO.transform.localScale =
                targetScale;

            group.alpha = 1f;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < optionRevealDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    optionRevealDuration);

            // Ease Out Back suave:
            // aparece rápido y tiene un pequeño "pop".
            const float overshoot = 1.15f;

            float c1 =
                overshoot;

            float c3 =
                c1 + 1f;

            float eased =
                1f +
                c3 *
                Mathf.Pow(t - 1f, 3f) +
                c1 *
                Mathf.Pow(t - 1f, 2f);

            buttonGO.transform.localScale =
                Vector3.LerpUnclamped(
                    initialScale,
                    targetScale,
                    eased);

            group.alpha = t;

            yield return null;
        }

        buttonGO.transform.localScale =
            targetScale;

        group.alpha = 1f;
    }

    private void SetButtonsSelectable(
        bool selectable)
    {
        foreach (GameObject buttonGO in spawnedButtons)
        {
            if (buttonGO == null)
                continue;

            CanvasGroup group =
                buttonGO.GetComponent<CanvasGroup>();

            if (group != null)
            {
                group.interactable =
                    selectable;

                group.blocksRaycasts =
                    selectable;
            }

            Button button =
                buttonGO.GetComponentInChildren<Button>(true);

            if (button != null)
                button.interactable = selectable;
        }
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
        if (!selectionUnlocked ||
            optionSelected ||
            selectedAbility == null ||
            runInventory == null)
        {
            return;
        }

        if (!runInventory.AddOrUpgrade(selectedAbility))
        {
            Debug.LogWarning($"No se pudo agregar/mejorar {selectedAbility.abilityName}.");
            return;
        }

        CompleteSelection();
    }

    public void ApplyHealOption(float healPercent)
    {
        if (!selectionUnlocked ||
            optionSelected ||
            playerContext == null ||
            playerContext.PlayerController == null)
        {
            return;
        }

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
