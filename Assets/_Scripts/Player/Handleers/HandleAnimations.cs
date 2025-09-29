using UnityEngine;

public class HandleAnimations : MonoBehaviour
{
    Animator animator;
    private string currentAnimation;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("No se encontró Animator en " + gameObject.name);
    }
 
    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimation == newState) return;

        // PLAY THE ANIMATION //
        currentAnimation = newState;
        animator.CrossFadeInFixedTime(currentAnimation, 0.2f);
    }
}
