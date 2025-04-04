using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject camHolder;
    [SerializeField] private float speed, maxForce, mouseSens, jumpForce;
    private Vector2 move, look;
    private float lookRotation;
    Rigidbody rb;
    Animator animator;
    AudioSource audioSource;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        Attack();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        //   Jump();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        SetAnimations();
    }

    private void LateUpdate()
    {
        LookWithMouse();
    }

    private void FixedUpdate()
    {
        Move();
    }


    private void LookWithMouse()
    {
        //Turn
        transform.Rotate(Vector3.up * look.x * mouseSens);

        // Look
        lookRotation += (-look.y * mouseSens);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
    }
    private void Move()
    {
        // Find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= speed;

        targetVelocity = transform.TransformDirection(targetVelocity); // Aling direction

        Vector3 velocityChange = (targetVelocity - currentVelocity); // Calculate force & fix falling
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, maxForce); // Limit Force

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    private void Jump()
    {
        Vector3 jumpForces = Vector3.zero;
        if (IsGrounded())
        {
            jumpForces = Vector3.up * jumpForce;
        }

        rb.AddForce(jumpForces, ForceMode.VelocityChange);
    }
    private bool IsGrounded()
    {
        bool hit = Physics.Raycast((transform.position - new Vector3(0, -1, 0)), Vector3.down, .7f, LayerMask.GetMask("Ground"));
        Debug.DrawRay((transform.position - new Vector3(0, -1, 0)), Vector3.down, Color.blue, .7f);
        return hit;
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")]
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackSpeed = 1f;
    //  [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;

    [Header("Debug")]

    [SerializeField] bool attacking = false;
    [SerializeField] bool readyToAttack = true;
    [SerializeField] int attackCount;

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
       // audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 10);
    }


    // HANDLE ANIMATIONS //

    string currentAnimation;
    public const string IDLE = "Idle";
    public const string ATTACK1 = "Attack1";
    public const string ATTACK2 = "Attack2";

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
        if (!attacking)
            ChangeAnimationState(IDLE);
    }
}


