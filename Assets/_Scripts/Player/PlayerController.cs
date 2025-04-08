using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject camHolder;
    [SerializeField] private float speed, maxForce, mouseSens, jumpForce;
    private float lookRotation;
    private Rigidbody rb;
    private PlayerContext playerContext;
 
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


    private void LookWithMouse()
    {
        //Turn
        Vector2 look = playerContext.handleInputs.GetLookVector2();
        transform.Rotate(Vector3.up * look.x * mouseSens);

        // Look
        lookRotation += (-look.y * mouseSens);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
    }
    private void Move()
    {
        Vector2 move = playerContext.handleInputs.GetMoveVector2();
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
}


