using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject camHolder;
    [SerializeField] public PlayerStats playerStats;

    private PlayerContext playerContext;
    private Rigidbody rb;
    private Collider playerCollider;
    private float currentHealth;
    public float currentSpeed;
    private float lookRotation;
    private bool isDashing;
    private bool canTakeDamage;
    private Vector3 dashDirection;
    private float dashSpeed;
    private float lastDashTime = -Mathf.Infinity;

    public float CurrentHealth { get => currentHealth; private set { } }
    public int MaxHealth { get => playerStats.maxHealth; private set { } }
    public float RunningSpeed { get => playerStats.runningSpeed; private set { } }
    public float WalkingSpeed { get => playerStats.runningSpeed; private set { } }

    [Header("Charged Jump")]
    [SerializeField, Min(0.05f)]
    private float chargedJumpFullChargeTime = 0.8f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField, Min(0.01f)] private float groundedExtraDistance = 0.12f;
    [SerializeField, Range(0.2f, 1f)] private float groundSphereRadiusFactor = 0.8f;

    public float currentJumpCharge = 0f;

    private bool isChargingJump;
    private float jumpChargeElapsed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        playerContext = GetComponent<PlayerContext>();

        // PlayerStats es un ScriptableObject. Creamos una copia runtime para que
        // las runas afecten sólo a esta run y nunca modifiquen el asset base.
        if (playerStats != null)
            playerStats = Instantiate(playerStats);

        if (groundMask.value == 0)
            groundMask = LayerMask.GetMask("Ground");
        currentHealth = playerStats.maxHealth;
        canTakeDamage = true;
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
        if (!IsGrounded())
            return;

        if (!isChargingJump)
        {
            isChargingJump = true;
            jumpChargeElapsed = 0f;
            currentJumpCharge = playerStats.minJumpForce;
        }

        jumpChargeElapsed += Time.deltaTime;

        float charge01 = Mathf.Clamp01(
            jumpChargeElapsed /
            Mathf.Max(0.05f, chargedJumpFullChargeTime)
        );

        currentJumpCharge = Mathf.Lerp(
            playerStats.minJumpForce,
            playerStats.maxJumpForce,
            charge01
        );
    }

    public void StopChargingJump()
    {
        isChargingJump = false;
        jumpChargeElapsed = 0f;
        currentJumpCharge = 0f;
    }

    public void DoJump(float force)
    {
        isChargingJump = false;
        jumpChargeElapsed = 0f;

        float finalForce = Mathf.Clamp(
            force,
            playerStats.minJumpForce,
            playerStats.maxJumpForce
        );

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset Y
        rb.AddForce(Vector3.up * finalForce, ForceMode.Impulse);
        SoundManagerOcta.Instance.PlaySound("PlayerJump");
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
        int mask = groundMask.value != 0
            ? groundMask.value
            : LayerMask.GetMask("Ground");

        if (playerCollider == null)
        {
            // Fallback por si el collider no está en el mismo GameObject.
            return Physics.Raycast(
                transform.position,
                Vector3.down,
                2.2f + groundedExtraDistance,
                mask,
                QueryTriggerInteraction.Ignore
            );
        }

        Bounds bounds = playerCollider.bounds;

        float radius = Mathf.Max(
            0.05f,
            Mathf.Min(bounds.extents.x, bounds.extents.z) *
            groundSphereRadiusFactor
        );

        float castDistance = Mathf.Max(
            0.02f,
            bounds.extents.y - radius + groundedExtraDistance
        );

        bool grounded = Physics.SphereCast(
            bounds.center,
            radius,
            Vector3.down,
            out RaycastHit hit,
            castDistance,
            mask,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(
            bounds.center,
            Vector3.down * castDistance,
            grounded ? Color.green : Color.red
        );

        return grounded;
    }

    public void TakeDamage(float damage, DamageFeedbackType type = DamageFeedbackType.Normal)
    {
        if (!canTakeDamage) return;

        currentHealth -= damage;
        UIManager.Instance.OnPlayerTakeDamage();
        if (currentHealth <= 0)
            Die();
    }
    public void ChangeSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
    public void AddHealth(int health)
    {
        if (currentHealth >= playerStats.maxHealth)
        {
            return;
        }
        else
        {
            currentHealth += health;
            //playerStats.maxHealth += health; 
            UIManager.Instance.OnPlayerAddHealth(); // Flash verde en UI
        }

    }

    public void AddMaxHealth(int health)
    {
        if (health <= 0)
            return;

        currentHealth += health;
        playerStats.maxHealth += health;

        if (UIManager.Instance != null)
            UIManager.Instance.OnPlayerAddHealth();
    }

    public void AddMaxDamage(float damage)
    {
        if (damage <= 0f)
            return;

        playerStats.basicMaxDamage += damage;
    }

    public void AddMoveSpeed(float speed)
    {
        if (speed <= 0f)
            return;

        playerStats.runningSpeed += speed;

        // Aunque el movimiento actual usa runningSpeed como velocidad objetivo,
        // mantenemos maxSpeed acompasado para no dejar stats contradictorias.
        playerStats.maxSpeed += speed;

        // Si Thor ya estaba caminando/corriendo, la mejora se siente de inmediato.
        if (currentSpeed > 0f)
            currentSpeed += speed;
    }

    public void AddMaxJumpForce(float force)
    {
        playerStats.maxJumpForce += force;

        if (playerStats.maxJumpForce < playerStats.minJumpForce)
            playerStats.maxJumpForce = playerStats.minJumpForce;
    }
    protected virtual void Die()
    {
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.RestartDifficulty();
        }
        Debug.Log("Lose");
        canTakeDamage = false;
        GameManager.Instance.ChangeState(new LoseState());
    }
}


