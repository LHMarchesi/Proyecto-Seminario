using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMjolnirRetractBehavior
{
    void OnRetract(Mjolnir mjolnir);
}

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
    //public float parryCooldown;
    // public float nextParryCD;

    public Action<Collider> OnHitEnemy;
    public Action OnMjolnirThrow;
    public Action OnChrgingThrow;

    private bool isHeld;
    private bool isRetracting;
    public bool teleport;
    public bool parry;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 3;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;
    private Vector3 originalSize;


    private readonly List<IMjolnirRetractBehavior> retractBehaviors = new();
    public void RegisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (!retractBehaviors.Contains(behavior))
            retractBehaviors.Add(behavior);
    }
    public void UnregisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (retractBehaviors.Contains(behavior))
            retractBehaviors.Remove(behavior);
    }


    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
        originalSize = transform.localScale;
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

            throwChargeTime += Time.deltaTime *2;  // Increment time charge
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
            isRetracting = true;
    }

    private void FixedUpdate()
    {
        if (isRetracting)
        {
            Retract();
        }
    }

    public void Throw()
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

    public void ThrowWithPower(float powerMultiplier = 1f)
    {
        OnMjolnirThrow?.Invoke();

        isRetracting = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;


        Vector3 cameraForward = Camera.main.transform.forward;
        float finalThrowPower = maxThrowPower * powerMultiplier;
        Debug.Log(finalThrowPower);

        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    public void ShowMessage(string message) { Debug.Log(message); }

    private void Retract()
    {
        if (isHeld) return;     // Avoid running if already held

        foreach (var behavior in retractBehaviors)
        {
            behavior.OnRetract(this);
        }


        // if (teleport == true)
        // {
        //      audio.PlayOneShot((AudioClip)Resources.Load("teleportVFX"));
        //      playerContext.PlayerController.transform.position = this.transform.position;
        //      Catch();
        //  }

        Vector3 directionToHand = hand.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToHand);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        rb.isKinematic = true; // Detenemos la física
        transform.position = Vector3.MoveTowards(transform.position, hand.position, maxRetractPower * Time.deltaTime);


        if (Vector3.Distance(hand.position, transform.position) < .5f)
        {
            Catch();
        }
        else if (Vector3.Distance(hand.position, transform.position) < .15f)
        {
            rb.isKinematic = false;
            Vector3 cameraForward = Camera.main.transform.forward;
            float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);
            isRetracting = false;
            bool isCurrentlyThrowing = playerContext.HandleInputs.IsThrowing();
            rb.AddForce(cameraForward.normalized * finalThrowPower * 5f, ForceMode.VelocityChange);
            //  audio.PlayOneShot((AudioClip)Resources.Load("parryVFX"));

        }
        this.transform.localScale = originalSize;

        transform.localScale = Vector3.Lerp(transform.localScale, originalSize, Time.deltaTime * 10f);
    }

    void Catch()
    {
        isHeld = true;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
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

    public void StopRetracting()
    {
        isRetracting = false;
    }

    public bool IsHeld() { return isHeld; }

    public float DistanceFromHand() { return Vector3.Distance(hand.position, transform.position); }
}
