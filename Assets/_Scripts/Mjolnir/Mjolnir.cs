using System;
using UnityEngine;

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
    [SerializeField] private float maxRetractPower;
    [SerializeField] private float damage;

    public Action<Collider> OnHitEnemy;

    private bool isHeld;
    private bool isRetracting;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 1.5f;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;

    [Header("TestHoming")]
    [SerializeField] private GameObject target;
    [SerializeField] private float distance;
    [SerializeField] private float rotationForce;
    [SerializeField] private bool homingOn;
    [Header("TestSplit")]
    [SerializeField]private bool explodeOn;
    [SerializeField] private int explosionDamage;
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionForce;
    public GameObject explosion;
    public LayerMask whatIsEnemies;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
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
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            isRetracting = false;
        }
    }

    private void FixedUpdate()
    {
        if (isRetracting)
            Retract();
    }

    void Throw()
    {
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        isHeld = false;

        Vector3 cameraForward = Camera.main.transform.forward; // Calculate direction towards camera
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);  // Calculate the force acordding to the loading time

        transform.parent = null;

        // Apply force and torque
        if(homingOn == false)
        {
            rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
            rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
            isHeld = false;
        }
        if (homingOn == true)
        {
            //rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
            //rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
            target = FindClosestByTag("Enemy");
            distance = Vector3.Distance(transform.position, target.GetComponent<Transform>().position);
            if (distance < 10)
            {
                Vector3 direction = target.GetComponent<Transform>().position - rb.position;
                direction.Normalize();
                Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);
                rb.angularVelocity = rotationAmount * rotationForce;
                rb.velocity = transform.forward * finalThrowPower;
            }

            isHeld = false;
        }

    }

    public static GameObject MakeMjolnirCopy(GameObject original)
    {
        Mjolnir newMjolnir = Instantiate(original).GetComponent<Mjolnir>();
        newMjolnir.isHeld = false;
        return newMjolnir.gameObject;

        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    void Retract()
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
                Debug.Log("HAGO PUN");
                boxCollider.isTrigger = true;
                //if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);
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
    }

    private void OnCollisionExit(Collision collision)
    {
        boxCollider.isTrigger = false;
    }
            damageable?.TakeDamage(damage);
            OnHitEnemy?.Invoke(collision.collider);
        }
    }

    public bool IsHeld() { return isHeld; }
}
