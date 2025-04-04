using UnityEngine;

public class HandleAnimations : MonoBehaviour
{
    Animator animator;
    HandleAttack handleAttack;
    [SerializeField] private string currentAnimation;
    public const string IDLE = "Idle";
    public const string ATTACK1 = "Attack1";
    public const string ATTACK2 = "Attack2";

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        handleAttack = GetComponent<HandleAttack>();
    }

    void Update()
    {
        SetAnimations();
    }

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimation == newState) return;

        // PLAY THE ANIMATION //
        currentAnimation = newState;
        animator.CrossFadeInFixedTime(currentAnimation, 0.2f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if (!handleAttack.isAttacking())
            ChangeAnimationState(IDLE);
    }
}
