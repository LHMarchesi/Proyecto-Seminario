using System;
using UnityEngine;

public class PlayerContext : MonoBehaviour // Save valuable data from the player
{
    private HandleInputs handleInputs;
    private HandleAnimations handleAnimations;
    private PlayerController playerController;
    private Mjolnir mjolnir;

    public Mjolnir Mjolnir { get => mjolnir; set => mjolnir = value; }
    public HandleAnimations HandleAnimations { get => handleAnimations; set => handleAnimations = value; }
    public HandleInputs HandleInputs { get => handleInputs; set => handleInputs = value; }
    public PlayerController PlayerController { get => playerController; set => playerController = value; }

    private void OnEnable()
    {
        HandleInputs = GetComponent<HandleInputs>();
        HandleAnimations = GetComponentInChildren<HandleAnimations>();
        playerController = GetComponent<PlayerController>();
        Mjolnir = GetComponentInChildren<Mjolnir>();
    }

    public Action OnUpdate;

    void Update()
    {
        OnUpdate?.Invoke();
    }
}
