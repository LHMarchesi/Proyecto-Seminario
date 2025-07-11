using UnityEngine;
using UnityEngine.UI;

public class HabilityIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;

    private float cooldownTime;
    private float startTime;
    private bool isOnCooldown = false;

    public void Initialize(Sprite sprite)
    {
        iconImage.sprite = sprite;
        cooldownOverlay.fillAmount = 0f; // No hay cooldown al inicio
    }

    public void TriggerCooldown(float cooldown)
    {
        cooldownTime = cooldown;
        startTime = Time.time;
        isOnCooldown = true;
        cooldownOverlay.fillAmount = 1f;
    }

    private void Update()
    {
        if (!isOnCooldown) return;

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