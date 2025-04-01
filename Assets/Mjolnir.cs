using UnityEngine;

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;

    [Header("References")]
    [SerializeField] private Transform hand;
  
    [Header("Settings")]
    [SerializeField] private float throwPower;
    [SerializeField] private float retractPower;
    [SerializeField] private float damage;

    [Header("Throwing Keys")]
    public KeyCode throwKey;
    public KeyCode retractKey;

    [Header("Debug")]
    [SerializeField] private bool isHeld;
    [SerializeField] private bool isRetracting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Catch();
    }

    private void OnCollisionEnter3D(Collision collision)
    {
        // check if you hit an enemy
        if (collision.collider.CompareTag("Enemy"))
        {
            // enemy = collision.gameObject.GetComponent<BasicEnemyDone>();

         //   enemy.TakeDamage(damage);
        }
    }

    void Update()
    {
        if (isHeld && Input.GetKeyDown(throwKey))
        {
            Throw();
        }
        else if (!isHeld && Input.GetKey(retractKey))
        {
            isRetracting = true;
        }
        else if (!isHeld && Input.GetKeyUp(retractKey))
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

        Vector3 dir = transform.parent.forward;
        transform.parent = null;

        rb.AddForce(dir * throwPower, ForceMode.VelocityChange);
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
        isHeld = true;
        isRetracting = false;

        rb.velocity = Vector3.zero; // Reset rb values
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.parent = hand; // Assing to the hand
        transform.position = hand.position;
    }
}
