using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStatsTabScreen : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject statsPanel; // Canvas o Panel que mostrará las estadísticas
    public PlayerController player; // referencia al script del jugador

    [Header("Textos UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI runSpeedText;
    public TextMeshProUGUI walkSpeedText;

    [Header("Botones")]
    public Button increaseHealthButton;
    public Button increaseRunSpeedButton;
    public Button increaseWalkSpeedButton;

    [Header("Ajustes de Mejora")]
    public float healthIncreaseAmount = 10f;
    public float runSpeedIncreaseAmount = 0.5f;
    public float walkSpeedIncreaseAmount = 0.25f;

    private bool isOpen = false;

    private void Start()
    {
        statsPanel.SetActive(false);

        // Asignar funciones a los botones
        increaseHealthButton.onClick.AddListener(() => IncreaseStat("Health"));
        increaseRunSpeedButton.onClick.AddListener(() => IncreaseStat("RunSpeed"));
        increaseWalkSpeedButton.onClick.AddListener(() => IncreaseStat("WalkSpeed"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatsPanel();
        }

        if (isOpen)
        {
            UpdateUI();
        }
    }

    private void ToggleStatsPanel()
    {
        isOpen = !isOpen;
        statsPanel.SetActive(isOpen);
    }

    private void UpdateUI()
    {
        if (player == null) return;

        healthText.text = $"Max Health: {player.MaxHealth:F0}";
        runSpeedText.text = $"Run Speed: {player.RunningSpeed:F2}";
        walkSpeedText.text = $"Walk Speed: {player.WalkingSpeed:F2}";
    }

    private void IncreaseStat(string stat)
    {
        if (player == null) return;

        switch (stat)
        {
            case "Health":
                player.MaxHealth += healthIncreaseAmount;
                break;
            case "RunSpeed":
                player.RunningSpeed += runSpeedIncreaseAmount;
                break;
            case "WalkSpeed":
                player.WalkingSpeed += walkSpeedIncreaseAmount;
                break;
        }

        UpdateUI();
    }
}
