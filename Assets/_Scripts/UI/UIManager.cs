using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public SliderPassValue PowerSlider { get => powerSlider; set => powerSlider = value; }
    public SliderPassValue HealthSlider { get => healthSlider; set => healthSlider = value; }
    public TextMeshProUGUI StateText { get => stateText; set => stateText = value; }

    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private SliderPassValue powerSlider;
    [SerializeField] private SliderPassValue healthSlider;
    [SerializeField] private TextMeshProUGUI stateText;
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
        DontDestroyOnLoad(gameObject);

        HealthSlider.ChangeValue(playerContext.PlayerController.MaxHealth);
    }

    private void ShowDamageFlash()
    {
        StopAllCoroutines(); 
        StartCoroutine(DamageFlashCoroutine());
    }

    private IEnumerator DamageFlashCoroutine()
    {
        damagePanel.gameObject.SetActive(true); // Aparece con alfa fuerte

        Color color = damagePanel.color;
        color.a = 0.6f;
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
}
