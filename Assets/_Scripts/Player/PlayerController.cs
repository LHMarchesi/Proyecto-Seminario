using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject camHolder;
    [SerializeField] public PlayerStats playerStats;

    private PlayerContext playerContext;
    private Rigidbody rb;
    private float currentHealth;
    public float currentSpeed;
    private float lookRotation;
    private bool isDashing;
    private Vector3 dashDirection;
    private float dashSpeed;
    private float lastDashTime = -Mathf.Infinity;

    public float CurrentHealth { get => currentHealth; private set { } }
    public float MaxHealth { get => playerStats.maxHealth; private set { } }
    public float RunningSpeed { get => playerStats.runningSpeed; private set { } }
    public float WalkingSpeed { get => playerStats.walkingSpeed; private set { } }

    private float currentJumpCharge = 0f;

    private bool isChargingJump;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponent<PlayerContext>();
        currentHealth = playerStats.maxHealth;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        LookWithMouse();
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * playerStats.extraGravityForce, ForceMode.VelocityChange);

        Move();
    }
    public void Dash(Vector3 dir, float speed)
    {
        isDashing = true;
        dashDirection = dir;
        dashSpeed = speed;
    }
    public void EndDash()
    {
        lastDashTime = Time.time;
        isDashing = false;
    }
    public bool CanDash()
    {
        return Time.time >= lastDashTime + playerStats.dashCooldown;
    }
    private void LookWithMouse()
    {
        if (GameManager.Instance.GetCurrentState() is PauseState)
            return;

        //Turn
        Vector2 look = playerContext.HandleInputs.GetLookVector2();
        transform.Rotate(Vector3.up * look.x * playerStats.mouseSens);

        // Look
        lookRotation += (-look.y * playerStats.mouseSens);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y, camHolder.transform.eulerAngles.z);
    }
    private void Move()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            return;
        }

        // Si está cargando salto  ignoramos input y frenamos
        if (isChargingJump)
        {
            // calcular el porcentaje de carga (0 = inicio, 1 = carga máxima)
            float chargePercent = currentJumpCharge / playerStats.maxJumpForce;

            // frenar según la carga
            float slowFactor = Mathf.Lerp(1f, 0.3f, chargePercent);

            Vector2 moveX = playerContext.HandleInputs.GetMoveVector2();

            Vector3 tarVelocity = new Vector3(moveX.x, 0, moveX.y) * currentSpeed;
            tarVelocity = transform.TransformDirection(tarVelocity);

            // factor de frenado
            tarVelocity *= slowFactor;

            rb.velocity = new Vector3(tarVelocity.x, rb.velocity.y, tarVelocity.z);
            return;
        }


        Vector2 move = playerContext.HandleInputs.GetMoveVector2();
        // Find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= currentSpeed;

        targetVelocity = transform.TransformDirection(targetVelocity); // Aling direction

        Vector3 velocityChange = (targetVelocity - currentVelocity); // Calculate force & fix falling
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        Vector3.ClampMagnitude(velocityChange, playerStats.maxSpeed); // Limit Speed

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    public void ChargingJump()
    {
        if (!IsGrounded()) return;

        isChargingJump = true;
        currentJumpCharge += playerStats.chargeSpeed * Time.fixedDeltaTime;
        currentJumpCharge = Mathf.Clamp(currentJumpCharge, playerStats.minJumpForce, playerStats.maxJumpForce);
    }
    public void DoJump()
    {
        if (!isChargingJump) return;

        float finalForce = Mathf.Max(currentJumpCharge, playerStats.minJumpForce);

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset Y
        rb.AddForce(Vector3.up * finalForce, ForceMode.Impulse);

        currentJumpCharge = 0f;
        isChargingJump = false;
    }
    public bool IsFalling()
    {
        return rb.velocity.y < -0.1f && !IsGrounded();
    }
    public bool HasMinimumAirHeight(float minHeight)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            float distanceToGround = hit.distance;
            return distanceToGround >= minHeight;
        }
        return false;
    }
    public Rigidbody GetRigidbody()
    {
        return rb;
    }
    public bool IsGrounded()
    {
        Vector3 boxCenter = transform.position + Vector3.down * 1f;
        Vector3 boxHalfExtents = new Vector3(0.3f, 0.1f, 0.3f); // ajustalo según el tamaño de tu personaje
        bool hit = Physics.CheckBox(boxCenter, boxHalfExtents, Quaternion.identity, LayerMask.GetMask("Ground"));

        Debug.DrawLine(boxCenter + Vector3.left * boxHalfExtents.x, boxCenter + Vector3.right * boxHalfExtents.x, Color.red, 0.1f);
        Debug.DrawLine(boxCenter + Vector3.forward * boxHalfExtents.z, boxCenter + Vector3.back * boxHalfExtents.z, Color.red, 0.1f);

        return hit;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UIManager.Instance.OnPlayerTakeDamage();
        if (currentHealth <= 0)
            Die();
    }
    public void ChangeSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
    public void AddHealth(float health)
    {
        currentHealth += health;
        playerStats.maxHealth += health;
        UIManager.Instance.OnPlayerAddHealth(); // Flash verde en UI
    }
    public void AddMaxDamage(float damage)
    {
        playerStats.basicMaxDamage += damage;
    }
    public void AddMaxJumpForce(float force)
    {
        playerStats.minJumpForce += force;
    }
    protected virtual void Die()
    {
        Debug.Log("Lose");
        GameManager.Instance.ChangeState(new LoseState());
    }
}


