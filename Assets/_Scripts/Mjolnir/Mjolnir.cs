using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerContext playerContext;
    private Quaternion startRotation;
    public BoxCollider boxCollider;

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

    [Header("TestHoming")]
    [SerializeField] private GameObject target;
    [SerializeField] private float distance;
    [SerializeField] private float rotationForce;
    [SerializeField] private bool homingOn;
    [Header("TestExplosion")]
    [SerializeField]private bool explodeOn;
    [SerializeField] private int explosionDamage;
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionForce;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    [Header("TestPull")]
    [SerializeField] private bool pullOn;
    [SerializeField] private float pullRange;
    [SerializeField] private float distance2;
    [SerializeField] private float maxPullRange;
    [SerializeField] private float pullForce;


    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        playerContext = GetComponentInParent<PlayerContext>();
        startRotation = transform.rotation;
        Catch();
    }

    public GameObject FindClosestByTag(string Enemy)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(Enemy);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;

            }
        }
        Debug.Log(closest.name);
        return closest;
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

        if (pullOn == false)
        {
            rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
            rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
            isHeld = false;
        }
        if(pullOn == true)
        {
            target = FindClosestByTag("Enemy");
            distance2 = Vector3.Distance(transform.position, target.GetComponent<Transform>().position);
            if(distance < pullRange)
            {
                target.GetComponent<Rigidbody>().AddForce((target.transform.position - transform.position).normalized * -pullForce, ForceMode.Impulse);
            }
        }

    }

    public static GameObject MakeMjolnirCopy(GameObject original)
    {
        Mjolnir newMjolnir = Instantiate(original).GetComponent<Mjolnir>();
        newMjolnir.isHeld = false;
        return newMjolnir.gameObject;
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

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();

        if (damageable == playerContext.PlayerController.GetComponent<IDamageable>()) //Dont damage player
            return;
        Debug.Log("Choque con" + collision.collider.name);
        if (damageable != null)
        {
            if(explodeOn == false)
            {
                boxCollider.isTrigger = true;
                damageable?.TakeDamage(damage);
            }
            if (explodeOn == true)
            {
                
                boxCollider.isTrigger = true;
                if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);
                Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
                for (int i = 0; i < enemies.Length; i++)
                {
                    enemies[i].GetComponent<IDamageable>().TakeDamage(explosionDamage);
                    if (enemies[i].GetComponent<Rigidbody>())
                    {
                        GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
                    }
                }
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxPullRange);
    }

    private void OnCollisionExit(Collision collision)
    {
        boxCollider.isTrigger = false;
    }

    public bool IsHeld() { return isHeld; }
}
