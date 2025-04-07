using UnityEngine;
using UnityEngine.UI;

public class SliderPassValue : MonoBehaviour
{
    [SerializeField] private Slider slider;
    void Start()
    {
        slider.enabled = false;
    }

    public void ChangeValue(float value)
    {
        slider.enabled = true;
        slider.value = value;
    }

    public void Disable()
    {
        slider.enabled = false;
    }
}
