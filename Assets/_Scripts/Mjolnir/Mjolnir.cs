using System;
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
    [SerializeField] private float maxRetractPower;
    [SerializeField] private float damage;
    [SerializeField] private float homingStrength;
    [SerializeField] private float homingDuration = 1f;

    public Action<Collider> OnHitEnemy;
    public Action OnMjolnirThrow;

    private bool isHeld;
    private bool isRetracting;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 1.5f;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
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
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        OnMjolnirThrow?.Invoke();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        isHeld = false;

        Vector3 cameraForward = Camera.main.transform.forward; // Calculate direction towards camera
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);  // Calculate the force acordding to the loading time

        transform.parent = null;

        // Apply force and torque
        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    public void ForceRetract(float force)
    {
        maxRetractPower = force;
        isRetracting = true;
    }

    private void Retract()
    {
        if (isHeld) return; // Avoid running if already held

        Vector3 directionToHand = hand.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToHand);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        rb.isKinematic = true; // Detenemos la física
        transform.position = Vector3.MoveTowards(transform.position, hand.position, maxRetractPower * Time.deltaTime);

        if (Vector3.Distance(hand.position, transform.position) < .5f)
        {
            Catch();
        }
    }

    void Catch()
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        isHeld = true;
        isRetracting = false;

        transform.parent = hand; // Assing to the hand
        transform.localPosition = Vector3.zero;
        transform.localRotation = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isHeld) return;

        IDamageable damageable = collision.collider.GetComponent<IDamageable>();

        if (damageable == playerContext.PlayerController.GetComponent<IDamageable>()) //Dont damage player
            return;

        if (damageable != null)
        {
            damageable?.TakeDamage(damage);
            OnHitEnemy?.Invoke(collision.collider);
        }
    }


    public bool IsHeld() { return isHeld; }
}
