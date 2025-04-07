using UnityEngine;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    public SliderPassValue powerSlider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Evita duplicados
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Opcional: mantener entre escenas
    }
}
