using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public SliderPassValue PowerSlider { get => powerSlider; set => powerSlider = value; }
    public SliderPassValue HealthSlider { get => healthSlider; set => healthSlider = value; }

    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private SliderPassValue powerSlider;
    [SerializeField] private SliderPassValue healthSlider;
    [SerializeField] private TextMeshProUGUI enemiesRemainingTxt;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image damagePanel;

    [Header("Build Items")]
    [SerializeField] private Transform habilidadesPanel;
    [SerializeField] private GameObject habilityIconPrefab;

    [SerializeField] private SliderPassValue bossHealth;
    [SerializeField] private TextMeshProUGUI bossNameTxt;

    private Image PauseScreen;
    [SerializeField] private GameObject LoseScreen;
    [SerializeField] private GameObject WinScreen;

    private readonly Dictionary<string, HabilityIcon> habilityIcons = new Dictionary<string, HabilityIcon>();

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

        SoundManagerOcta.Instance.PlayMusic("GameTheme");
    }

    private void Start()
    {
        PowerSlider.Disable();
        HealthSlider.ChangeValue(playerContext.PlayerController.MaxHealth);
        healthText.text = playerContext.PlayerController.MaxHealth + "/" + playerContext.PlayerController.MaxHealth;
        PauseScreen = GameObject.FindGameObjectWithTag("PauseScreen")?.GetComponent<Image>();
        TogglePauseScreen(false);
    }

    // --- BUILD ITEM UI ---

    // Wrapper legacy para que powerups viejos sigan compilando.
    public void RegisterHability(string id, Sprite sprite)
    {
        RegisterHability(id, sprite, 1);
    }

    public void RegisterHability(string id, Sprite sprite, int level)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (habilityIcons.TryGetValue(id, out HabilityIcon existingIcon))
        {
            existingIcon.SetLevel(level);
            return;
        }

        if (habilityIconPrefab == null || habilidadesPanel == null)
            return;

        GameObject iconGO = Instantiate(habilityIconPrefab, habilidadesPanel);
        HabilityIcon icon = iconGO.GetComponent<HabilityIcon>();

        if (icon == null)
        {
            Destroy(iconGO);
            Debug.LogError("UIManager: habilityIconPrefab no contiene HabilityIcon.");
            return;
        }

        icon.Initialize(sprite, level);
        habilityIcons.Add(id, icon);
    }

    public void SetHabilityLevel(string id, int level)
    {
        if (habilityIcons.TryGetValue(id, out HabilityIcon icon))
            icon.SetLevel(level);
    }

    public void TriggerHabilityCooldown(string id, float cooldown)
    {
        if (habilityIcons.TryGetValue(id, out HabilityIcon icon))
            icon.TriggerCooldown(cooldown);
    }

    public void ClearRunHabilitiesUI()
    {
        foreach (HabilityIcon icon in habilityIcons.Values)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }

        habilityIcons.Clear();
    }

    // --- RESTO DE UI EXISTENTE ---

    public void ChangeRemainingEnemiesText(string text)
    {
        enemiesRemainingTxt.text = text;
    }

    public void SetBossName(string name)
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

    public void SetBossHealth(float health)
    {
        bossHealth.ChangeValue(health);
    }

    public void UpdateEnemiesRemaining(bool show, int count)
    {
        enemiesRemainingTxt.gameObject.SetActive(show);

        if (count == 0)
            enemiesRemainingTxt.text = "Door Open";
        else
            enemiesRemainingTxt.text = "Pillars remaining: " + count;
    }

    private void ShowDamageFlash()
    {
        StopAllCoroutines();
        StartCoroutine(PanelFlashCoroutine(Color.red));
    }

    private void ShowHealthFlash()
    {
        StopAllCoroutines();
        StartCoroutine(PanelFlashCoroutine(Color.green));
    }

    public void TogglePauseScreen(bool value)
    {
        if (PauseScreen == null)
            return;

        PauseScreen.gameObject.SetActive(value);

        if (value)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Time.timeScale = value ? 0f : 1f;
    }

    public void ShowLoseScreenn(bool value)
    {
        if (LoseScreen != null)
            LoseScreen.SetActive(value);
    }

    public void ShowWinScreenn(bool value)
    {
        if (WinScreen != null)
            WinScreen.SetActive(value);
    }

    private IEnumerator PanelFlashCoroutine(Color color)
    {
        if (damagePanel == null)
            yield break;

        damagePanel.gameObject.SetActive(true);
        color.a = 0.3f;
        damagePanel.color = color;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.6f, 0f, elapsed / duration);
            damagePanel.color = color;
            yield return null;
        }

        damagePanel.gameObject.SetActive(false);
    }

    public void OnPlayerTakeDamage()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        healthText.text = playerContext.PlayerController.CurrentHealth + "/" + playerContext.PlayerController.MaxHealth;
        ShowDamageFlash();
    }

    public void OnPlayerAddHealth()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        healthText.text = playerContext.PlayerController.CurrentHealth + "/" + playerContext.PlayerController.MaxHealth;
        ShowHealthFlash();
    }
}
