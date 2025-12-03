using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerStatsTabScreen : MonoBehaviour
{
    [Header("Aumento de estadisitcas por StatPoint")]
    public int maxHealthIncrease;
    public float maxWalkSpeedIncrease;
    public float damageIncrease;
    public float mjolnirIncrease;

    [Header("References")]
    [SerializeField] private PlayerStats defaultPlayerStats;
    [SerializeField] private PlayerStats currentPlayerStats;
    [SerializeField] private Mjolnir mjolnir;

    [SerializeField] private GameObject tabPanel;
    [SerializeField] private ExperienceManager experienceManager;

    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI runningSpeedText;
    [SerializeField] private TextMeshProUGUI mjolnirDamageText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI statPointsText;


    [SerializeField] private Button increaseHealthButton;
    [SerializeField] private Button increaseWalkSpeedButton;
    [SerializeField] private Button increaseRunSpeedButton;
    [SerializeField] private Button increaseMjolnirButton;

    private bool isOpen = false;
    private PlayerContext playerContext;

    void Start()
    {
        playerContext = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContext>();
        

        SetStatsToDefault(); 
        tabPanel.SetActive(false);
        experienceManager.OnLevelUp += UpdateUI;
        AddButtonListeners();
    }

    void Update()
    {
        // Mostrar el panel solo mientras se mantenga TAB
        if (Keyboard.current.tabKey.isPressed)
        {
            if (!isOpen  && GameManager.Instance.currentState == GameStates.Game)
                OpenPanel();
        }
        else if (isOpen)
        {
            ClosePanel();
        }

        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Cursor.lockState = CursorLockMode.None;  // Libera el cursor
            Cursor.visible = true;                   // Lo hace visible
        }

        // Cuando suelta Alt
        if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor
            Cursor.visible = false;                   // Lo oculta
        }
    }

    void AddButtonListeners()
    {
        increaseHealthButton.onClick.AddListener(IncreaseHealth);
        increaseWalkSpeedButton.onClick.AddListener(IncreaseWalkSpeed);
        increaseRunSpeedButton.onClick.AddListener(IncreaseRunSpeed);
        increaseMjolnirButton.onClick.AddListener(IncreaseMjolnirAttack);
    }

    void OpenPanel()
    {
        playerContext.HandleInputs.SetPaused(true);
        isOpen = true;
        tabPanel.SetActive(true);
        UpdateUI();


        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ClosePanel()
    {
        playerContext.HandleInputs.SetPaused(false);
        isOpen = false;
        tabPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UpdateUI()
    {
        maxHealthText.text = $"Health: {defaultPlayerStats.maxHealth:F1}";
        runningSpeedText.text = $"Running Speed: {defaultPlayerStats.runningSpeed:F1}";
        damageText.text = $"Melee Damage: {defaultPlayerStats.basicMaxDamage:F1}";
        statPointsText.text = $"Upgrade Points: {experienceManager.GetAvailableStatPoints()}";
        mjolnirDamageText.text = $"Mjolnir Damage: {mjolnir.damage:F1}";
    }

    void IncreaseHealth()
    {
        if (!experienceManager.SpendStatPoint()) return;
        defaultPlayerStats.maxHealth += maxHealthIncrease;
        UpdateUI();
        SoundManagerOcta.Instance.PlaySound("SpendPoint");
    }

    void IncreaseMjolnirAttack()
    {
        if (!experienceManager.SpendStatPoint()) return;
        mjolnir.damage += mjolnirIncrease;
        UpdateUI();
        SoundManagerOcta.Instance.PlaySound("SpendPoint");
    }

    void IncreaseWalkSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        defaultPlayerStats.runningSpeed += maxWalkSpeedIncrease;
        UpdateUI();
        SoundManagerOcta.Instance.PlaySound("SpendPoint");
    }

    void IncreaseRunSpeed()
    {
        if (!experienceManager.SpendStatPoint()) return;
        defaultPlayerStats.basicMaxDamage += damageIncrease;
        UpdateUI();
        SoundManagerOcta.Instance.PlaySound("SpendPoint");
    }

    private void SetStatsToDefault()
    {
        currentPlayerStats.maxHealth = defaultPlayerStats.maxHealth;
        currentPlayerStats.runningSpeed = defaultPlayerStats.runningSpeed;
        currentPlayerStats.maxSpeed = defaultPlayerStats.maxSpeed;
        currentPlayerStats.dashCooldown = defaultPlayerStats.dashCooldown;
        currentPlayerStats.minJumpForce = defaultPlayerStats.minJumpForce;
        currentPlayerStats.maxJumpForce = defaultPlayerStats.maxJumpForce;
        currentPlayerStats.chargeSpeed = defaultPlayerStats.chargeSpeed;
        currentPlayerStats.chargeSlowMultiplier = defaultPlayerStats.chargeSlowMultiplier;
        currentPlayerStats.basicMaxDamage = defaultPlayerStats.basicMaxDamage;
    }
}
