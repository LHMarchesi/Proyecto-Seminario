using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TransitionType
{
    FadeIn,
    FadeOut
}

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Animator Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private TransitionType startTransition = TransitionType.FadeOut;
    private HandleAnimations HandleAnimations;

    private bool isTransitioning;

    private void Awake()
    {
        // Ensure there's only one instance of GameManager (Singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        HandleAnimations = GetComponent<HandleAnimations>();
    }


    private void Start()
    {
        if (playOnStart)
            PlayTransition(startTransition);
    }

    /// <summary>
    /// Llama una animación de transición por su tipo.
    /// </summary>
    public void PlayTransition(TransitionType type)
    {
        if (isTransitioning)
            return;

        HandleAnimations.ChangeAnimationState(type.ToString());

    }

 
    public void PlayTransitionAndLoadScene(TransitionType type, int sceneIndex)
    {
        StartCoroutine(PlayTransitionAndLoadSceneCoroutine(type, sceneIndex));
    }

    public IEnumerator PlayTransitionAndLoadSceneCoroutine(TransitionType type, int sceneIndex)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        HandleAnimations.ChangeAnimationState(type.ToString());
        yield return null;

        float fadeOutDuration = HandleAnimations.GetCurrentAnimationLength();
        yield return new WaitForSeconds(fadeOutDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;

        HandleAnimations.ChangeAnimationState(TransitionType.FadeOut.ToString());
        yield return null;
        yield return new WaitForSeconds(HandleAnimations.GetCurrentAnimationLength());

        isTransitioning = false;
    }
}
