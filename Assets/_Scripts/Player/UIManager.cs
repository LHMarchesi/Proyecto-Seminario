using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public SliderPassValue powerSlider;
    public TextMeshProUGUI stateText;
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
}
