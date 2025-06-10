using UnityEngine;
using UnityEngine.UI;

public class SliderPassValue : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private bool disableOnEnable;
    void OnEnable()
    {
        slider = GetComponent<Slider>();
    }

    public void ChangeValue(float value)
    {
        slider.gameObject.SetActive(true);
        if (value > slider.maxValue)
            slider.maxValue = value;

        slider.value = value;

    }

    public void Disable()
    {
        slider.value = 0;
        slider.gameObject.SetActive(false);
    }
}
