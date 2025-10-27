using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExperienceManager : MonoBehaviour
{
    [Header("PlayerContext")]
    [SerializeField] PlayerContext playerContext;

    [Header("Habilidades")]
    public List<AbilityEntry> availableAbilities = new List<AbilityEntry>();

    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;

    float currentLevel, totalExperience;
    float previousLevelsExperience, nextLevelsExperience;

    [Header("Stat Points")]
    [Tooltip("Puntos de mejora disponibles para gastar al subir de nivel")]
    public int availableStatPoints = 3;

    [Tooltip("Cantidad de puntos de mejora otorgados por nivel")]
    public int statPointsPerLevel = 3;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image experienceFill;

    [Header("Panel de Elección de Habilidades")]
    [SerializeField] GameObject panel;
    [SerializeField] Transform abilityButtonContainer;
    [SerializeField] GameObject abilityButtonPrefab;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    //  Evento para que otros scripts (como la UI) se actualicen
    public delegate void OnLevelUpEvent();
    public event OnLevelUpEvent OnLevelUp;

    public void AddExperience(float amount)
    {
        totalExperience += amount;
        CheckForLevelUp();
        UpdateInterface();
    }

    void CheckForLevelUp()
    {
        if (totalExperience >= nextLevelsExperience)
        {
            currentLevel++;
            UpdateLevel();
            LevelUp();
        }
    }

    void LevelUp()
    {
        // Otorgar puntos de mejora
        availableStatPoints += statPointsPerLevel;

        // Disparar evento
        OnLevelUp?.Invoke();

        // Lógica de elección de habilidades
        panel.SetActive(true);
        StartCoroutine(pauseWDelay());
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerContext.HandleInputs.SetPaused(true);

        List<AbilityEntry> options = GetRandomAbilityOptions(2);

        foreach (var ability in options)
        {
            GameObject buttonGO = Instantiate(abilityButtonPrefab, abilityButtonContainer);
            spawnedButtons.Add(buttonGO);

            AbilityButtonUI buttonUI = buttonGO.GetComponent<AbilityButtonUI>();
            buttonUI.Setup(ability, this);
        }
    }

    public void ApplySelectedAbility(AbilityEntry selectedAbility)
    {
        GameObject instance = Instantiate(selectedAbility.abilityPrefab);
        BasePowerUp powerUp = instance.GetComponent<BasePowerUp>();
        playerContext.HandleInputs.SetPaused(false);

        if (powerUp != null)
        {
            powerUp.SetPlayerContext(playerContext);
            powerUp.PickUp();
        }

        // 🔹 Limpiar botones UI
        foreach (var go in spawnedButtons)
            Destroy(go);
        spawnedButtons.Clear();

        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }

    void UpdateLevel()
    {
        float curveValue = experienceCurve.Evaluate(currentLevel);
        previousLevelsExperience = 100 * Mathf.Pow(currentLevel, 1.5f) * curveValue;
        nextLevelsExperience = 100 * Mathf.Pow(currentLevel + 1, 1.5f) * experienceCurve.Evaluate(currentLevel + 1);
        UpdateInterface();
    }

    void UpdateInterface()
    {
        float start = totalExperience - previousLevelsExperience;
        float end = nextLevelsExperience - previousLevelsExperience;

        experienceFill.fillAmount = (float)start / (float)end;

        if (levelText != null)
            levelText.text = $"Nivel {currentLevel}";
    }

    List<AbilityEntry> GetRandomAbilityOptions(int count)
    {
        List<AbilityEntry> shuffled = new List<AbilityEntry>(availableAbilities);
        shuffled.Shuffle();
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }

    public bool SpendStatPoint()
    {
        if (availableStatPoints <= 0)
            return false;

        availableStatPoints--;
        return true;
    }

    public int GetAvailableStatPoints() => availableStatPoints;


    IEnumerator pauseWDelay()
    {
        yield return new WaitForSecondsRealtime(.3f);
        Time.timeScale = 0;
    }
}



[System.Serializable]
public class AbilityEntry
{
    public string abilityName;
    public string abilityDescription;
    public GameObject abilityPrefab; // Prefab que contiene el PowerUp (LightningStrike, Explode, etc.)
    public Sprite icon;
    [Range(1, 100)]
    public float dropChance;

}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

}
