using UnityEngine;

public class TransitionController : MonoBehaviour
{
    private Animator animator;
    private RuntimeAnimatorController baseController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        baseController = animator.runtimeAnimatorController;
    }

    private void Start()
    {
        //Obtén los niveles de la transición
        var (from, to) = GameManager.Instance.GetPendingTransition();

        string clipName = $"Transition_{from}_{to}";
        
        // Carga ese clip desde Resources 
        var clip = Resources.Load<AnimationClip>($"Transitions/{clipName}");
        if (clip == null)
        {
            Debug.LogError($"No se encontró animación {clipName}");
            return;
        }

        // Crea un override controller y sobreescribe el clip placeholder
        var overrideController = new AnimatorOverrideController(baseController);
        overrideController["PlaceHolder"] = clip;
        animator.runtimeAnimatorController = overrideController;

        animator.SetTrigger("PlayTransition");
    }

    // Animation Event al final del clip:
    public void AnimacionTerminada()
    {
        if (GameManager.Instance.GetCurrentState() is TransitionState ts) 
            ts.OnTransitionComplete();  // Event OnTransitionComplete
    }
}
