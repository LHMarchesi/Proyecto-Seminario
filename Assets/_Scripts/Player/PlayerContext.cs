using UnityEngine;

public class PlayerContext : MonoBehaviour // Save valuable data from the player
{
    public HandleInputs handleInputs;
    public HandleAnimations handleAnimations;
    public Mjolnir mjolnir;

    private void OnEnable()
    {
        handleInputs = GetComponent<HandleInputs>();
        handleAnimations = GetComponent<HandleAnimations>();
        mjolnir = GetComponentInChildren<Mjolnir>();
    }
}
