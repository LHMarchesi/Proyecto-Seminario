using System.Collections;
using UnityEngine;

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerContext playerContext;
    private Animator animator;

    [Header("References")]
    [SerializeField] private Transform hand;

    [Header("Settings")]
    [SerializeField] private float minThrowPower;
    [SerializeField] private float maxThrowPower;
    [SerializeField] private float torqueForce;
    [SerializeField] private float retractPower;
    [SerializeField] private float damage;

    [Header("Debug")]
    [SerializeField] private bool isHeld;
    [SerializeField] private bool isRetracting;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 1.5f;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerContext = GetComponentInParent<PlayerContext>();
        Catch();
    }

    void Update()
    {
        // HADNLE THROW //
        //
        bool isCurrentlyThrowing = playerContext.handleInputs.IsThrowing();
        if (isHeld && isCurrentlyThrowing)
        {
            if (!isChargingThrow)
                isChargingThrow = true;  // Start charging the throw

            throwChargeTime += Time.deltaTime;  // Increment time charge
            throwChargeTime = Mathf.Clamp(throwChargeTime, 0f, maxChargeTime);
        }
        else if (isChargingThrow && wasThrowing && !isCurrentlyThrowing) // Throw at button release
        {
            Throw();
            throwChargeTime = 0f;  // Reset throw
            isChargingThrow = false;
        }
        wasThrowing = isCurrentlyThrowing; // Save state 


        // HADNLE CATCH //
        //
        if (!isHeld && playerContext.handleInputs.IsCatching())
        {
            isRetracting = true;
        }
        else if (!isHeld && !playerContext.handleInputs.IsCatching())
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
        StartCoroutine(WaitForCurrentAnimationEnd());
        animator.enabled = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 cameraForward = Camera.main.transform.forward; // Calculate direction towards camera
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);  // Calculate the force acordding to the loading time

        transform.parent = null;

        // Apply force and torque
        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
        isHeld = false;
    }

    void Retract()
    {
        if (Vector3.Distance(hand.position, transform.position) < 1) // If close enough
        {
            Catch();
        }

        rb.isKinematic = false;
        Vector3 directionToHand = hand.position - transform.position;
        rb.AddForce(directionToHand.normalized * retractPower, ForceMode.VelocityChange);

        // Limit max Velocity
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, retractPower);
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

    public bool IsHeld() { return isHeld; }

    private IEnumerator WaitForCurrentAnimationEnd()
    {
        // Obtener el nombre del estado actual (o el �ndice si lo prefieres)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Esperar mientras la animaci�n est� en progreso
        while (stateInfo.normalizedTime < 1f)  // NormalizedTime es un valor entre 0 y 1
        {
            // Actualizamos la informaci�n del estado
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null; // Esperamos un frame
        }
    }
}
