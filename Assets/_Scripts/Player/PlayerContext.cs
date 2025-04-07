using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public Mjolnir mjolnir;
    public HandleInputs Inputs;
    public HandleAnimations handleAnimations;

    private void OnEnable()
    {
        Inputs = GetComponent<HandleInputs>();
        handleAnimations = GetComponent<HandleAnimations>();
        mjolnir = GetComponentInChildren<Mjolnir>();
    }
}
