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
    public float sizeForce;
    public float parryCooldown;
    public float nextParryCD;

    public Action<Collider> OnHitEnemy;
    public Action OnMjolnirThrow;

    private bool isHeld;
    private bool isRetracting;
    public bool sizeChange;
    public bool teleport;
    public bool parry;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 1.5f;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;
    [SerializeField] public Vector3 originalSize;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
        Catch();
        originalSize = this.transform.localScale;
    }

    public void TeleportEnable()
    {
        teleport = true;
    }
    public void SizeEnable()
    {
        sizeChange = true;
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
            Debug.Log(throwChargeTime);
            if(sizeChange == true)
            {
                this.transform.localScale = new Vector3(1.5f + (throwChargeTime * sizeForce), 1.5f + (throwChargeTime * sizeForce), 1.5f + (throwChargeTime * sizeForce));
                //this.transform.Rotate(1.5f, 1.5f, 1.5f);
                this.transform.eulerAngles = new Vector3(240, -40, 70);
            }
                
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
       /* else if (!isHeld && !playerContext.HandleInputs.IsCatching())
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            isRetracting = false;
        }*/
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
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.volume = 0.25f;
        if (isHeld) return; // Avoid running if already held
        if(teleport == true)
        {
            audio.PlayOneShot((AudioClip)Resources.Load("teleportVFX"));
            playerContext.PlayerController.transform.position = this.transform.position;
            Catch();
        }
        else if(teleport == false)
        {
            Vector3 directionToHand = hand.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToHand);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            rb.isKinematic = true; // Detenemos la física
            transform.position = Vector3.MoveTowards(transform.position, hand.position, maxRetractPower * Time.deltaTime);
        }

        if (Vector3.Distance(hand.position, transform.position) < .5f && parry == false)
        {
            Catch();
        }
        else if (Vector3.Distance(hand.position, transform.position) < .15f && parry == true)
        {
            rb.isKinematic = false;
            Vector3 cameraForward = Camera.main.transform.forward;
            float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);
            isRetracting = false;
            bool isCurrentlyThrowing = playerContext.HandleInputs.IsThrowing();
            rb.AddForce(cameraForward.normalized * finalThrowPower * 5f, ForceMode.VelocityChange);
            audio.PlayOneShot((AudioClip)Resources.Load("parryVFX"));

        }
        this.transform.localScale = originalSize;
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
