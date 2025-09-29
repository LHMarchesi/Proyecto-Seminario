using UnityEngine;
using UnityEngine.UI;

public class SliderPassValue : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private bool disableOnEnable;

    [Header("Efecto Delay")]
    [SerializeField] private Image delayedFill;  // Asignalo en el Inspector
    [SerializeField] private float smoothSpeed = 2f;

    private float targetValue;

    void OnEnable()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (delayedFill != null)
        {
            float currentValue = delayedFill.fillAmount * slider.maxValue;
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);
            delayedFill.fillAmount = currentValue / slider.maxValue;
        }
    }

    public void ChangeValue(float value)
    {
        slider.gameObject.SetActive(true);

        if (value > slider.maxValue)
            slider.maxValue = value;

        slider.value = value;
        targetValue = value; // actualizamos también el objetivo del efecto
    }

    public void Disable()
    {
        slider.value = 0;
        slider.gameObject.SetActive(false);
    }
}