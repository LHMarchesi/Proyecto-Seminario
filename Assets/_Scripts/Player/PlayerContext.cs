using UnityEngine;

public class PlayerContext : MonoBehaviour // Save valuable data from the player
{
    private HandleInputs handleInputs;
    private HandleAnimations handleAnimations;
    private Mjolnir mjolnir;

    public Mjolnir Mjolnir { get => mjolnir; set => mjolnir = value; }
    public HandleAnimations HandleAnimations { get => handleAnimations; set => handleAnimations = value; }
    public HandleInputs HandleInputs { get => handleInputs; set => handleInputs = value; }

    private void OnEnable()
    {
        HandleInputs = GetComponent<HandleInputs>();
        HandleAnimations = GetComponent<HandleAnimations>();
        Mjolnir = GetComponentInChildren<Mjolnir>();
    }
}
