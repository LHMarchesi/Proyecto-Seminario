using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceManager : MonoBehaviour
{
    [Header("Habilidades")]
    public List<AbilityEntry> availableAbilities = new List<AbilityEntry>();

    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;

    int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image experienceFill;

    [Header("Panel de Elección de Habilidades")]
    [SerializeField] GameObject panel;
    [SerializeField] Transform abilityButtonContainer;
    [SerializeField] GameObject abilityButtonPrefab;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    void Start()
    {
        UpdateLevel();
    }

    public void AddExperience(int amount)
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

    AbilityEntry GetRandomAbility()
    {
        int randomNumber = Random.Range(1, 101);
        List<AbilityEntry> possibleAbilities = new List<AbilityEntry>();

        foreach (var ability in availableAbilities)
        {
            if (randomNumber <= ability.dropChance)
            {
                possibleAbilities.Add(ability);
            }
        }

        if (possibleAbilities.Count > 0)
        {
            return possibleAbilities[Random.Range(0, possibleAbilities.Count)];
        }

        Debug.LogWarning("No ability selected");
        return null;
    }

    void LevelUp()
    {
        panel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;

        List<AbilityEntry> options = GetRandomAbilityOptions(2); // Elegimos 2 opciones al azar

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

        if (powerUp != null)
        {
            powerUp.SetPlayerContext(this.GetComponent<PlayerContext>());
            powerUp.PickUp();
        }

        // Limpiar botones UI
        foreach (var go in spawnedButtons)
            Destroy(go);
        spawnedButtons.Clear();

        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    void UpdateLevel()
    {
        previousLevelsExperience = (int)experienceCurve.Evaluate(currentLevel);
        nextLevelsExperience = (int)experienceCurve.Evaluate(currentLevel + 1);
        UpdateInterface();
    }

    void UpdateInterface()
    {
        int start = totalExperience - previousLevelsExperience;
        int end = nextLevelsExperience - previousLevelsExperience;

        levelText.text = currentLevel.ToString();
        experienceFill.fillAmount = (float)start / (float)end;
    }

    List<AbilityEntry> GetRandomAbilityOptions(int count)
    {
        List<AbilityEntry> shuffled = new List<AbilityEntry>(availableAbilities);
        shuffled.Shuffle();
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }
}

[System.Serializable]
public class AbilityEntry
{
    public string abilityName;
    public GameObject abilityPrefab; // Prefab que contiene el PowerUp (LightningStrike, Explode, etc.)
    public Sprite icon;
    [Range(1, 100)]
    public int dropChance;

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
