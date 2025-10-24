using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerStatsTabScreen : MonoBehaviour
{
    [Header("Aumento de estadisitcas por StatPoint")]
    public float maxHealthIncrease;
    public float maxWalkSpeedIncrease;
    public float maxRunSpeedIncrease;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject tabPanel;
    [SerializeField] private ExperienceManager experienceManager;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI walkingSpeedText;
    [SerializeField] private TextMeshProUGUI runningSpeedText;
    [SerializeField] private TextMeshProUGUI statPointsText;
    [SerializeField] private Button increaseHealthButton;
    [SerializeField] private Button increaseWalkSpeedButton;
    [SerializeField] private Button increaseRunSpeedButton;

    private bool isOpen = false;

    void Start()
    {
        tabPanel.SetActive(false);
        experienceManager.OnLevelUp += UpdateUI;
        AddButtonListeners();
    }

    void Update()
    {
        // Mostrar el panel solo mientras se mantenga TAB
        if (Keyboard.current.tabKey.isPressed)
        {
            Debug.Log("Tab key is being pressed.");
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
        increaseHealthButton.onClick.AddListener(IncreaseHealth);
        increaseWalkSpeedButton.onClick.AddListener(IncreaseWalkSpeed);
        increaseRunSpeedButton.onClick.AddListener(IncreaseRunSpeed);
    }

    void OpenPanel()
    {
        //playerContext.HandleInputs.SetPaused(true);
        isOpen = true;
        tabPanel.SetActive(true);
        UpdateUI();


        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ClosePanel()
    {
        isOpen = false;
        tabPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UpdateUI()
    {
        maxHealthText.text = $"Max Health: {playerStats.maxHealth:F1}";
        walkingSpeedText.text = $"Walking Speed: {playerStats.walkingSpeed:F1}";
        runningSpeedText.text = $"Running Speed: {playerStats.runningSpeed:F1}";
        statPointsText.text = $"Stat Points: {experienceManager.GetAvailableStatPoints()}";
    }

    void IncreaseHealth()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerStats.maxHealth += maxHealthIncrease;
        UpdateUI();
    }

    void IncreaseWalkSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerStats.walkingSpeed += maxWalkSpeedIncrease;
        UpdateUI();
    }

    void IncreaseRunSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        playerStats.walkingSpeed += maxRunSpeedIncrease;
        UpdateUI();
    }
}
