using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HabilityIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI levelText;

    private float cooldownTime;
    private float startTime;
    private bool isOnCooldown;

    public void Initialize(Sprite sprite)
    {
        Initialize(sprite, 1);
    }

    public void Initialize(Sprite sprite, int level)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        SetLevel(level);
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }

    public void TriggerCooldown(float cooldown)
    {
        if (cooldownOverlay == null || cooldown <= 0f)
            return;

        cooldownTime = cooldown;
        startTime = Time.time;
        isOnCooldown = true;
        cooldownOverlay.fillAmount = 1f;
    }

    private void Update()
    {
        if (!isOnCooldown || cooldownOverlay == null)
            return;

        float elapsed = Time.time - startTime;
        float ratio = Mathf.Clamp01(elapsed / cooldownTime);
        cooldownOverlay.fillAmount = 1f - ratio;

        if (ratio >= 1f)
        {
            isOnCooldown = false;
            cooldownOverlay.fillAmount = 0f;
        }
    }
}
