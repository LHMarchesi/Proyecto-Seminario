using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public SliderPassValue PowerSlider { get => powerSlider; set => powerSlider = value; }
    public SliderPassValue HealthSlider { get => healthSlider; set => healthSlider = value; }

    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private SliderPassValue powerSlider;
    [SerializeField] private SliderPassValue healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image damagePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Singleton UI Manager
            return;
        }

        Instance = this;
     
    }

    private void Start()
    {
        PowerSlider.Disable();
        HealthSlider.ChangeValue(playerContext.PlayerController.MaxHealth);
        healthText.text = playerContext.PlayerController.MaxHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
    }
    private void ShowDamageFlash()
    {
        StopAllCoroutines(); 
        StartCoroutine(PanelFlashCoroutine(Color.red));
    } 
    private void ShowHealthFlash()
    {
        StopAllCoroutines(); 
        StartCoroutine(PanelFlashCoroutine(Color.green));
    }

    private IEnumerator PanelFlashCoroutine(Color color)
    {
        damagePanel.gameObject.SetActive(true); // Aparece con alfa fuerte

        damagePanel.color = color;
        color.a = 0.3f;
        damagePanel.color = color;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.6f, 0f, elapsed / duration); // Fade out
            damagePanel.color = color;
            yield return null;
        }
         
        damagePanel.gameObject.SetActive(false); // Desaparece
    }

    public void OnPlayerTakeDamage()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        healthText.text = playerContext.PlayerController.CurrentHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
        ShowDamageFlash();
    }

    public void OnPlayerAddHealth()
    {
        HealthSlider.ChangeValue(playerContext.PlayerController.CurrentHealth);
        healthText.text = playerContext.PlayerController.CurrentHealth.ToString() + "/" + playerContext.PlayerController.MaxHealth.ToString();
        ShowHealthFlash();
    }
}
