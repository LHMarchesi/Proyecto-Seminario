using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public SliderPassValue powerSlider;
    public TextMeshProUGUI stateText;
    public Image damagePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Singleton UI Manager
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void ShowDamageFlash()
    {
        StopAllCoroutines(); // Detenemos si había uno ya corriendo
        StartCoroutine(DamageFlashCoroutine());
    }

    private IEnumerator DamageFlashCoroutine()
    {
        damagePanel.gameObject.SetActive(true);

        Color color = damagePanel.color;
        color.a = 0.6f; // Aparece con alfa fuerte
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

        damagePanel.gameObject.SetActive(false);
    }
}
