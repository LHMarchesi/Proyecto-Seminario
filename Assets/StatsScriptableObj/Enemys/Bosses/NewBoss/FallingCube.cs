using UnityEngine;

public class FallingCube : MonoBehaviour
{
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.AddForce(Vector3.down * 500f, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();
        if (damageable == null) return;

        // Si el cubo golpea desde arriba, la normal del contacto apunta hacia arriba (aprox Vector3.up)
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        // Queremos que el cubo dañe solo si golpea al jugador desde arriba
        // (es decir, la normal está bastante cerca de Vector3.up)
        if (Vector3.Dot(normal, Vector3.up) > 0.5f)
        {
            damageable.TakeDamage(20);
        }
    }
}
