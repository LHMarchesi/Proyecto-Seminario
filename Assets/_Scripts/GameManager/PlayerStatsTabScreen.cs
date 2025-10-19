using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStatsTabScreen : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public ExperienceManager experienceManager;
    public GameObject statsPanel;

    [Header("Stat Display")]
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI walkingSpeedText;
    public TextMeshProUGUI runningSpeedText;
    public TextMeshProUGUI statPointsText;

    [Header("TMP Buttons")]
    public TextMeshProUGUI increaseHealthButton;
    public TextMeshProUGUI increaseWalkSpeedButton;
    public TextMeshProUGUI increaseRunSpeedButton;

    private bool isOpen = false;

    void Start()
    {
        statsPanel.SetActive(false);
        experienceManager.OnLevelUp += UpdateUI;
        AddButtonListeners();
    }

    void Update()
    {
        // ✅ Mostrar el panel solo mientras se mantenga TAB
        if (Keyboard.current.tabKey.isPressed)
        {
            if (!isOpen)
                OpenPanel();
        }
        else if (isOpen)
        {
            ClosePanel();
        }
    }

    void AddButtonListeners()
    {
        increaseHealthButton.GetComponentInParent<UnityEngine.UI.Button>().onClick.AddListener(IncreaseHealth);
        increaseWalkSpeedButton.GetComponentInParent<UnityEngine.UI.Button>().onClick.AddListener(IncreaseWalkSpeed);
        increaseRunSpeedButton.GetComponentInParent<UnityEngine.UI.Button>().onClick.AddListener(IncreaseRunSpeed);
    }

    void OpenPanel()
    {
        isOpen = true;
        statsPanel.SetActive(true);
        UpdateUI();

        // 🔹 Mostrar cursor y desbloquearlo
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ClosePanel()
    {
        isOpen = false;
        statsPanel.SetActive(false);

        // 🔹 Ocultar cursor y volver a bloquearlo
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UpdateUI()
    {
        maxHealthText.text = $"Max Health: {playerController.MaxHealth:F1}";
        walkingSpeedText.text = $"Walking Speed: {playerController.WalkingSpeed:F1}";
        runningSpeedText.text = $"Running Speed: {playerController.RunningSpeed:F1}";
        statPointsText.text = $"Stat Points: {experienceManager.GetAvailableStatPoints()}";
    }

    void IncreaseHealth()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerController.AddHealth(playerController.MaxHealth + 10f);
        UpdateUI();
    }

    void IncreaseWalkSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerController.ChangeSpeed(playerController.WalkingSpeed + 1f);
        UpdateUI();
    }

    void IncreaseRunSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerController.ChangeSpeed(playerController.RunningSpeed + 2f);
        UpdateUI();
    }
}
