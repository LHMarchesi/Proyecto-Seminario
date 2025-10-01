using UnityEngine;

public class DamageFloor : MonoBehaviour
{
    public float damage = 10f;     
    public float launchForce = 5f;   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            var player = collision.gameObject.GetComponent<PlayerController>(); // Cambia PlayerController por el script de tu Player

            if (player != null)
            {
                // Aplicar daño
                player.TakeDamage(damage);

                // Aplicar fuerza hacia arriba si tiene Rigidbody
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // Resetear Y para que el salto sea consistente
                    rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>(); // Cambia PlayerController por tu script real
            if (player != null)
            {
                player.TakeDamage(damage);

                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                }
            }
        }
    }
}
