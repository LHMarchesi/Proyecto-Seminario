using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AbilityButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image iconImage;
    public GameObject iTooltip;
    public TextMeshProUGUI abilityDescription;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Button selectButton;

    private ExperienceManager manager;
    private AbilityEntry ability;
    private bool isHealOption;
    private float healPercent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (iTooltip != null)
            iTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (iTooltip != null)
            iTooltip.SetActive(false);
    }

    public void Setup(AbilityEntry ability, ExperienceManager manager)
    {
        this.ability = ability;
        this.manager = manager;
        isHealOption = false;

        int currentLevel = manager.GetAbilityLevel(ability);
        int nextLevel = currentLevel + 1;

        if (iconImage != null)
            iconImage.sprite = ability.icon;

        if (nameText != null)
            nameText.text = ability.abilityName;

        if (levelText != null)
        {
            levelText.text = currentLevel == 0
                ? "NUEVO · Lv.1"
                : $"Lv.{currentLevel} ? Lv.{nextLevel}";
        }

        if (abilityDescription != null)
            abilityDescription.text = ability.GetDescriptionForLevel(nextLevel);

        ConfigureButton();
    }

    public void SetupHeal(
        ExperienceManager manager,
        Sprite icon,
        string displayName,
        string description,
        float healPercent)
    {
        this.manager = manager;
        this.ability = null;
        this.healPercent = healPercent;
        isHealOption = true;

        if (iconImage != null)
            iconImage.sprite = icon;

        if (nameText != null)
            nameText.text = displayName;

        if (levelText != null)
            levelText.text = "RECUPERACIÓN";

        if (abilityDescription != null)
            abilityDescription.text = $"{description}\nRecupera {Mathf.RoundToInt(healPercent * 100f)}% de HP máximo.";

        ConfigureButton();
    }

    private void ConfigureButton()
    {
        if (selectButton == null)
            return;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    public void OnSelect()
    {
        if (manager == null)
            return;

        if (isHealOption)
            manager.ApplyHealOption(healPercent);
        else
            manager.ApplySelectedAbility(ability);
    }
}