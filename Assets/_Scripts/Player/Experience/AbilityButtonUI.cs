using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButtonUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button selectButton;

    private ExperienceManager manager;
    private AbilityEntry ability;

    public void Setup(AbilityEntry ability, ExperienceManager manager)
    {
        this.ability = ability;
        this.manager = manager;

        iconImage.sprite = ability.icon;
        nameText.text = ability.abilityName;

        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        manager.ApplySelectedAbility(ability);
    }
}