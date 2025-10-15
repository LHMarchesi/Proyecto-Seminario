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
    [SerializeField] private Image damagePanel;

    [SerializeField] private Transform habilidadesPanel;
    [SerializeField] private GameObject habilityIconPrefab;

    private Image PauseScreen;
    [SerializeField] private GameObject LoseScreen;


    private Dictionary<string, HabilityIcon> habilityIcons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Singleton UI Manager
            return;
        }

        Instance = this;
        ShowLoseScreenn(false);
    }




    public void RegisterHability(string id, Sprite sprite)
    {
        if (habilityIcons.ContainsKey(id))
        {
            // Ya está registrada, hacé la mejora
            var existingPowerUp = GameObject.FindObjectsOfType<BasePowerUp>();
            foreach (var pu in existingPowerUp)
            {
                if (pu.name.Contains(id)) // Alternativamente podés hacer que cada PowerUp tenga un string `abilityId`
                {
                    pu.UpgradePowerUp();
                }
            }

            // Opcional: animación en el icono existente (ej: flash, escalarlo)
            return;
        }

        GameObject iconGO = Instantiate(habilityIconPrefab, habilidadesPanel);
        HabilityIcon icon = iconGO.GetComponent<HabilityIcon>();
        icon.Initialize(sprite);
        habilityIcons.Add(id, icon);
    }

    public void TriggerHabilityCooldown(string id, float cooldown)
    {
        if (habilityIcons.TryGetValue(id, out var icon))
        {
            icon.TriggerCooldown(cooldown);
        }
    }


    private void Start()
    {
        PowerSlider.Disable();
        HealthSlider.ChangeValue(playerContext.PlayerController.MaxHealth);
        //  healthText.text = playerContext.PlayerController.MaxHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
        PauseScreen = GameObject.FindGameObjectWithTag("PauseScreen")?.GetComponent<Image>();
        TogglePauseScreen(false);
        UpdateEnemiesRemaining(false, 0);
    }

    public void UpdateEnemiesRemaining(bool show,int count)
    {
        enemiesRemainingTxt.gameObject.SetActive(show); 

        if (count == 0)
            enemiesRemainingTxt.text = "Door Open";
        else
            enemiesRemainingTxt.text = "Enemies remaining: " + count.ToString();
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
        if (PauseScreen != null)
        {
            PauseScreen.gameObject.SetActive(value);
            if (value)
            {
                // Mostrar y desbloquear el cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Ocultar y bloquear el cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            Time.timeScale = value ? 0f : 1f; // Pausa o despausa el tiempo del juego
        }
    }

    public void ShowLoseScreenn(bool value)
    {
        LoseScreen.SetActive(value);
    }

    private IEnumerator PanelFlashCoroutine(Color color)
    {
        damagePanel.gameObject.SetActive(true); // Aparece con alfa fuerte

        damagePanel.color = color;
        color.a = 0.3f;
        damagePanel.color = color;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.6f, 0f, elapsed / duration); // Fade out
            damagePanel.color = color;
            yield return null;
        }

        damagePanel.gameObject.SetActive(false); // Desaparece
    }

    public void OnPlayerTakeDamage()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        //healthText.text = playerContext.PlayerController.CurrentHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
        ShowDamageFlash();
    }

    public void OnPlayerAddHealth()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        //    healthText.text = playerContext.PlayerController.CurrentHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
        ShowHealthFlash();
    }
}
