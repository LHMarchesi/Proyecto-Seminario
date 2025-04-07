using UnityEngine;
using UnityEngine.Windows;

public class PlayerContext : MonoBehaviour
{
    public Mjolnir mjolnir;
    public HandleInputs Inputs;
    public HandleAnimations handleAnimations;
    public SliderPassValue throwSlider;

    private void Awake()
    {
        Inputs = GetComponent<HandleInputs>();
        handleAnimations = GetComponent<HandleAnimations>();
        mjolnir = GetComponentInChildren<Mjolnir>();
    }
}
