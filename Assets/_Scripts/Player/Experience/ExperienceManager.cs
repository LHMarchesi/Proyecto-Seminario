using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceManager : MonoBehaviour
{
    [Header("Habilidades")]
    public List<ScriptableObject> abilityList = new List<ScriptableObject>();

    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;

    int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI experienceText;
    [SerializeField] Image experienceFill;
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI panelText;
    [SerializeField] TextMeshProUGUI panelDamageText;
    [SerializeField] TextMeshProUGUI panelHpText;
    [SerializeField] TextMeshProUGUI panelSpeedText;
    [SerializeField] Image panelImage;


    void Start()
    {
        UpdateLevel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AddExperience(3);
        }
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

    ScriptableObjectsAbilities GetRandomAbility()
    {
        int randomNumber = Random.Range(1, 101);
        List<ScriptableObjectsAbilities> possibleAbilities = new List<ScriptableObjectsAbilities>();
        foreach (ScriptableObjectsAbilities item in abilityList)
        {
            if (randomNumber <= item.dropChance)
            {
                possibleAbilities.Add(item);
            }
        }
        if (possibleAbilities.Count > 0)
        {
            ScriptableObjectsAbilities droppedAbility = possibleAbilities[Random.Range(0, possibleAbilities.Count)];
            return droppedAbility;
        }
        Debug.Log("No Ability");
        return null;
    }

    void LevelUp()
    {
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        GetRandomAbility();
        ScriptableObjectsAbilities droppedAbility = GetRandomAbility();
        if(droppedAbility != null)
        {
            panelText.text = droppedAbility.abilityName;
            panelImage.sprite = droppedAbility.lootSprite;
            

        }

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
        experienceText.text = start + " exp / " + end + " exp";
        experienceFill.fillAmount = (float)start / (float)end;
    }
}
