using UnityEngine;

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private HandleAnimations handleAnimations;
    private HandleInputs handleInputs;
    private Animator animator;

    [Header("References")]
    [SerializeField] private Transform hand;

    [Header("Settings")]
    [SerializeField] private float minThrowPower;         // Fuerza mínima al lanzar
    [SerializeField] private float maxThrowPower;
    [SerializeField] private float torqueForce;
    [SerializeField] private float retractPower;
    [SerializeField] private float damage;

    [Header("Debug")]
    [SerializeField] private bool isHeld;
    [SerializeField] private bool isRetracting;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 3f;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        handleAnimations = GetComponentInParent<HandleAnimations>();
        handleInputs = GetComponentInParent<HandleInputs>();
        Catch();
    }

    void Update()
    {
        bool isCurrentlyThrowing = handleInputs.IsThrowing();

        if (isHeld && isCurrentlyThrowing)
        {
            if (!isChargingThrow)
                isChargingThrow = true;  // Comienza a cargar el lanzamiento

            throwChargeTime += Time.deltaTime;  // Incrementa el tiempo de carga 
            throwChargeTime = Mathf.Clamp(throwChargeTime, 0f, maxChargeTime);
        }

        // Lanzar el Mjolnir al soltar el botón
        else if (isChargingThrow && wasThrowing && !isCurrentlyThrowing)
        {
            Throw();
            throwChargeTime = 0f;  // Resetea el tiempo de carga
            isChargingThrow = false;
        }

        wasThrowing = isCurrentlyThrowing; // Guarda el estado actual para la próxima iteración
        if (!isHeld && handleInputs.IsCatching())
        {
            isRetracting = true;
        }
        else if (!isHeld && !handleInputs.IsCatching())
        {
            isRetracting = false;
        }
    }

    private void FixedUpdate()
    {
        if (isRetracting)
        {
            Retract();
        }
    }

    void Throw()
    {
        animator.enabled = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 cameraForward = Camera.main.transform.forward; // Calcular la dirección del lanzamiento según la cámara
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);  // Calcular la fuerza del lanzamiento según el tiempo de carga

        transform.parent = null;

        // Aplicar la fuerza y el torque para el lanzamiento
        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
        isHeld = false;
    }

    void Retract()
    {
        if (Vector3.Distance(hand.position, transform.position) < 1)
        {
            Catch();
        }

        Vector3 directionToHand = hand.position - transform.position;
        rb.velocity = (directionToHand.normalized * retractPower);
    }

    void Catch()
    {
        animator.enabled = true;
        isHeld = true;
        isRetracting = false;

        rb.velocity = Vector3.zero; // Reset rb values
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.parent = hand; // Assing to the hand
        transform.position = hand.position;
    }
}
