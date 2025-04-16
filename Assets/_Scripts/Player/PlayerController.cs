using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject camHolder;
    [SerializeField] private float walkingSpeed, runningSpeed, maxForce, mouseSens, jumpForce;
    private float lookRotation;
    private Rigidbody rb;
    private PlayerContext playerContext;

    public float WalkingSpeed => walkingSpeed;
    public float RunningSpeed => runningSpeed;

    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponent<PlayerContext>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void LateUpdate()
    {
        LookWithMouse();
    }

    private void FixedUpdate()
    {
        Move();
    }
    private void Update()
    {
        Jump();
    }

    private void LookWithMouse()
    {
        //Turn
        Vector2 look = playerContext.HandleInputs.GetLookVector2();
        transform.Rotate(Vector3.up * look.x * mouseSens);

        // Look
        lookRotation += (-look.y * mouseSens);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
    }
    private void Move()
    {
        Vector2 move = playerContext.HandleInputs.GetMoveVector2();
        // Find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= currentSpeed;

        targetVelocity = transform.TransformDirection(targetVelocity); // Aling direction

        Vector3 velocityChange = (targetVelocity - currentVelocity); // Calculate force & fix falling
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, maxForce); // Limit Speed

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    private void Jump()
    {
        Vector3 jumpForces = Vector3.zero;
        if (IsGrounded() && playerContext.HandleInputs.IsJumping())
        {
            jumpForces = Vector3.up * jumpForce;
        }

        rb.AddForce(jumpForces, ForceMode.Impulse);
    }
    private bool IsGrounded()
    {
        Vector3 boxCenter = transform.position + Vector3.down * 1f;
        Vector3 boxHalfExtents = new Vector3(0.3f, 0.1f, 0.3f); // ajustalo según el tamaño de tu personaje
        bool hit = Physics.CheckBox(boxCenter, boxHalfExtents, Quaternion.identity, LayerMask.GetMask("Ground"));

        Debug.DrawLine(boxCenter + Vector3.left * boxHalfExtents.x, boxCenter + Vector3.right * boxHalfExtents.x, Color.red, 0.1f);
        Debug.DrawLine(boxCenter + Vector3.forward * boxHalfExtents.z, boxCenter + Vector3.back * boxHalfExtents.z, Color.red, 0.1f);

        return hit;
    }

    public void ChangeSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
}


