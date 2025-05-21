using System.Collections;
using UnityEngine;

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerContext playerContext;
    private Quaternion startRotation;

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
        playerContext = GetComponentInParent<PlayerContext>();
        startRotation = transform.rotation;
        Catch();
    }

    void Update()
    {
        // HADNLE THROW //
        //
        bool isCurrentlyThrowing = playerContext.HandleInputs.IsThrowing();
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
        if (!isHeld && playerContext.HandleInputs.IsCatching())
        {
            isRetracting = true;
        }
        else if (!isHeld && !playerContext.HandleInputs.IsCatching())
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
        if (isHeld) return; // Avoid running if already held

        // Ensure the rigidbody is not kinematic before applying physics
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (Vector3.Distance(hand.position, transform.position) < 1f)
        {
            Catch();
            return;
        }

        Vector3 directionToHand = hand.position - transform.position;
        rb.AddForce(directionToHand.normalized * retractPower, ForceMode.VelocityChange);

        // Limit max Velocity
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, retractPower);
    }
    void Catch()
    {
        isHeld = true;
        isRetracting = false;

        rb.velocity = Vector3.zero; // Reset rb values
        rb.angularVelocity = Vector3.zero; 
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.parent = hand; // Assing to the hand
        transform.SetPositionAndRotation(hand.position, startRotation);
    }

    public bool IsHeld() { return isHeld; }
}
