using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AbilityButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public GameObject iTooltip;
    public TextMeshProUGUI abilityDescription;
    public TextMeshProUGUI nameText;
    public Button selectButton;

    private ExperienceManager manager;
    private AbilityEntry ability;

    public void OnPointerEnter(PointerEventData eventData)
    {
        iTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        iTooltip.SetActive(false);
    }

    public void Setup(AbilityEntry ability, ExperienceManager manager)
    {
        this.ability = ability;
        this.manager = manager;

        iconImage.sprite = ability.icon;
        nameText.text = ability.abilityName;
        abilityDescription.text = ability.abilityDescription;

        selectButton.onClick.AddListener(OnSelect);
    }



    public void OnSelect()
    {
        manager.ApplySelectedAbility(ability);
        UIManager.Instance.OnPlayerAddHealth();
    }
}