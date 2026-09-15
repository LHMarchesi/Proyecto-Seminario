using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public SliderPassValue PowerSlider
    {
        get => powerSlider;
        set => powerSlider = value;
    }

    public SliderPassValue HealthSlider
    {
        get => healthSlider;
        set => healthSlider = value;
    }

    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private SliderPassValue powerSlider;
    [SerializeField] private SliderPassValue healthSlider;
    [SerializeField] private TextMeshProUGUI enemiesRemainingTxt;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image damagePanel;

    [Header("Build Items")]
    [SerializeField] private Transform habilidadesPanel;
    [SerializeField] private GameObject habilityIconPrefab;

    [Header("Rune Pickup Feedback")]
    [Tooltip("Texto que aparece al recoger una runa.")]
    [SerializeField] private TextMeshProUGUI runePickupText;

    [Tooltip("Opcional. Si lo asignas, hace fade del mensaje completo.")]
    [SerializeField] private CanvasGroup runePickupCanvasGroup;

    [SerializeField, Min(0.1f)] private float runePopupDuration = 1.25f;
    [SerializeField, Min(0f)] private float runeFadeInTime = 0.12f;
    [SerializeField, Min(0f)] private float runeFadeOutTime = 0.25f;

    [Header("Rune Colors")]
    [SerializeField]
    private Color healthRuneColor =
        new Color32(90, 230, 110, 255);

    [SerializeField]
    private Color damageRuneColor =
        new Color32(255, 100, 55, 255);

    [SerializeField]
    private Color speedRuneColor =
        new Color32(75, 210, 255, 255);

    [SerializeField]
    private Color jumpRuneColor =
        new Color32(165, 105, 255, 255);

    [Header("Boss")]
    [SerializeField] private SliderPassValue bossHealth;
    [SerializeField] private TextMeshProUGUI bossNameTxt;

    private Image PauseScreen;
    [SerializeField] private GameObject LoseScreen;
    [SerializeField] private GameObject WinScreen;

    private readonly Dictionary<string, HabilityIcon> habilityIcons =
        new Dictionary<string, HabilityIcon>();

    private Coroutine panelFlashRoutine;
    private Coroutine runePopupRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ShowLoseScreenn(false);
        ShowWinScreenn(false);
        DisableBossName();

        if (runePickupText != null)
            runePickupText.gameObject.SetActive(false);

        if (runePickupCanvasGroup != null)
            runePickupCanvasGroup.alpha = 0f;

        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.PlayMusic("GameTheme");
    }

    private void Start()
    {
        if (playerContext == null)
        {
            playerContext =
                GameObject.FindGameObjectWithTag("Player")
                    ?.GetComponent<PlayerContext>();
        }

        PowerSlider.Disable();

        if (playerContext != null &&
            playerContext.PlayerController != null)
        {
            HealthSlider.ChangeValue(
                playerContext.PlayerController.MaxHealth);

            healthText.text =
                playerContext.PlayerController.MaxHealth +
                "/" +
                playerContext.PlayerController.MaxHealth;
        }

        PauseScreen =
            GameObject.FindGameObjectWithTag("PauseScreen")
                ?.GetComponent<Image>();

        TogglePauseScreen(false);
    }

    // =====================================================
    // BUILD ITEM UI
    // =====================================================

    // Wrapper legacy para que powerups viejos sigan compilando.
    public void RegisterHability(
        string id,
        Sprite sprite)
    {
        RegisterHability(id, sprite, 1);
    }

    public void RegisterHability(
        string id,
        Sprite sprite,
        int level)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (habilityIcons.TryGetValue(
            id,
            out HabilityIcon existingIcon))
        {
            existingIcon.SetLevel(level);
            return;
        }

        if (habilityIconPrefab == null ||
            habilidadesPanel == null)
        {
            return;
        }

        GameObject iconGO =
            Instantiate(
                habilityIconPrefab,
                habilidadesPanel);

        HabilityIcon icon =
            iconGO.GetComponent<HabilityIcon>();

        if (icon == null)
        {
            Destroy(iconGO);

            Debug.LogError(
                "UIManager: habilityIconPrefab no contiene HabilityIcon.");

            return;
        }

        icon.Initialize(sprite, level);
        habilityIcons.Add(id, icon);
    }

    public void SetHabilityLevel(
        string id,
        int level)
    {
        if (habilityIcons.TryGetValue(
            id,
            out HabilityIcon icon))
        {
            icon.SetLevel(level);
        }
    }

    public void TriggerHabilityCooldown(
        string id,
        float cooldown)
    {
        if (habilityIcons.TryGetValue(
            id,
            out HabilityIcon icon))
        {
            icon.TriggerCooldown(cooldown);
        }
    }

    public void ClearRunHabilitiesUI()
    {
        foreach (HabilityIcon icon in
                 habilityIcons.Values)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }

        habilityIcons.Clear();
    }

    // =====================================================
    // RUNE PICKUP UI
    // =====================================================

    public void ShowRunePickup(
        BasicRuneStat stat,
        float amount)
    {
        if (runePickupText == null)
            return;

        string amountText =
            FormatRuneAmount(amount);

        string message;
        Color color;

        switch (stat)
        {
            case BasicRuneStat.MaxHealth:
                message =
                    "+" +
                    amountText +
                    " MAX HEALTH / +" +
                    amountText +
                    " HP";

                color = healthRuneColor;
                break;

            case BasicRuneStat.MeleeDamage:
                message =
                    "+" +
                    amountText +
                    " DAMAGE";

                color = damageRuneColor;
                break;

            case BasicRuneStat.MoveSpeed:
                message =
                    "+" +
                    amountText +
                    " MOVE SPEED";

                color = speedRuneColor;
                break;

            case BasicRuneStat.MaxJumpForce:
                message =
                    "+" +
                    amountText +
                    " JUMP FORCE";

                color = jumpRuneColor;
                break;

            default:
                message =
                    "+" +
                    amountText;

                color = Color.white;
                break;
        }

        if (runePopupRoutine != null)
        {
            StopCoroutine(runePopupRoutine);
            runePopupRoutine = null;
        }

        runePopupRoutine =
            StartCoroutine(
                RunePickupCoroutine(
                    message,
                    color));
    }

    private IEnumerator RunePickupCoroutine(
        string message,
        Color color)
    {
        runePickupText.text = message;
        runePickupText.color = color;
        runePickupText.gameObject.SetActive(true);

        if (runePickupCanvasGroup != null)
        {
            runePickupCanvasGroup.alpha = 0f;

            if (runeFadeInTime > 0f)
            {
                float elapsed = 0f;

                while (elapsed < runeFadeInTime)
                {
                    elapsed += Time.unscaledDeltaTime;

                    runePickupCanvasGroup.alpha =
                        Mathf.Clamp01(
                            elapsed /
                            runeFadeInTime);

                    yield return null;
                }
            }

            runePickupCanvasGroup.alpha = 1f;
        }

        float holdTime =
            Mathf.Max(
                0f,
                runePopupDuration -
                runeFadeInTime -
                runeFadeOutTime);

        if (holdTime > 0f)
        {
            yield return new WaitForSecondsRealtime(
                holdTime);
        }

        if (runePickupCanvasGroup != null &&
            runeFadeOutTime > 0f)
        {
            float elapsed = 0f;

            while (elapsed < runeFadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;

                runePickupCanvasGroup.alpha =
                    1f -
                    Mathf.Clamp01(
                        elapsed /
                        runeFadeOutTime);

                yield return null;
            }

            runePickupCanvasGroup.alpha = 0f;
        }

        runePickupText.gameObject.SetActive(false);
        runePopupRoutine = null;
    }

    private string FormatRuneAmount(float amount)
    {
        if (Mathf.Approximately(
            amount,
            Mathf.Round(amount)))
        {
            return Mathf.RoundToInt(amount)
                .ToString();
        }

        return amount.ToString("0.#");
    }

    // =====================================================
    // RESTO DE UI EXISTENTE
    // =====================================================

    public void ChangeRemainingEnemiesText(
        string text)
    {
        enemiesRemainingTxt.text = text;
    }

    public void SetBossName(
        string name)
    {
        bossNameTxt.gameObject.SetActive(true);
        bossHealth.gameObject.SetActive(true);
        bossNameTxt.text = name;
    }

    public void DisableBossName()
    {
        bossNameTxt.gameObject.SetActive(false);
        bossHealth.gameObject.SetActive(false);
    }

    public void SetBossHealth(
        float health)
    {
        bossHealth.ChangeValue(health);
    }

    public void UpdateEnemiesRemaining(
        bool show,
        int count)
    {
        enemiesRemainingTxt.gameObject.SetActive(show);

        if (count == 0)
            enemiesRemainingTxt.text = "Door Open";
        else
            enemiesRemainingTxt.text =
                "Pillars remaining: " + count;
    }

    private void ShowDamageFlash()
    {
        StartPanelFlash(Color.red);
    }

    private void ShowHealthFlash()
    {
        StartPanelFlash(Color.green);
    }

    private void StartPanelFlash(
        Color color)
    {
        if (panelFlashRoutine != null)
        {
            StopCoroutine(panelFlashRoutine);
            panelFlashRoutine = null;
        }

        panelFlashRoutine =
            StartCoroutine(
                PanelFlashCoroutine(color));
    }

    public void TogglePauseScreen(
        bool value)
    {
        if (PauseScreen == null)
            return;

        PauseScreen.gameObject.SetActive(value);

        if (value)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }

        Time.timeScale =
            value ? 0f : 1f;
    }

    public void ShowLoseScreenn(
        bool value)
    {
        if (LoseScreen != null)
            LoseScreen.SetActive(value);
    }

    public void ShowWinScreenn(
        bool value)
    {
        if (WinScreen != null)
            WinScreen.SetActive(value);
    }

    private IEnumerator PanelFlashCoroutine(
        Color color)
    {
        if (damagePanel == null)
        {
            panelFlashRoutine = null;
            yield break;
        }

        damagePanel.gameObject.SetActive(true);

        color.a = 0.3f;
        damagePanel.color = color;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            color.a =
                Mathf.Lerp(
                    0.6f,
                    0f,
                    elapsed / duration);

            damagePanel.color = color;

            yield return null;
        }

        damagePanel.gameObject.SetActive(false);
        panelFlashRoutine = null;
    }

    public void OnPlayerTakeDamage()
    {
        if (playerContext == null ||
            playerContext.PlayerController == null)
        {
            return;
        }

        HealthSlider.ChangeValue(
            playerContext.PlayerController.CurrentHealth);

        healthText.text =
            Mathf.CeilToInt(playerContext.PlayerController.CurrentHealth) +
            "/" +
            playerContext.PlayerController.MaxHealth;

        ShowDamageFlash();
    }

    public void OnPlayerAddHealth()
    {
        if (playerContext == null ||
            playerContext.PlayerController == null)
        {
            return;
        }

        HealthSlider.ChangeValue(
            playerContext.PlayerController.CurrentHealth);

        healthText.text =
            Mathf.CeilToInt(playerContext.PlayerController.CurrentHealth) +
            "/" +
            playerContext.PlayerController.MaxHealth;

        ShowHealthFlash();
    }
}
