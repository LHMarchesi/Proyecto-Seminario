using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingCube : MonoBehaviour
{
    public float fallForce = 500f;
    public float knockbackHorizontal;
    public float knockbackVertical;
    public float damage;
    [SerializeField] private BossHitbox areaHitboxOnImpact;

    private Rigidbody rb;
    private bool hasImpacted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.down * fallForce, ForceMode.Impulse);

        // Por si no lo asignaste en el inspector
        if (areaHitboxOnImpact == null)
            areaHitboxOnImpact = GetComponentInChildren<BossHitbox>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        // Detecta impacto con suelo
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasImpacted = true;

            // Activamos hitbox con parámetros deseados
            areaHitboxOnImpact.SetDamage(25f, 8f, 3f);
            areaHitboxOnImpact.EnableHitbox();

            // Desactivamos el hitbox después de un tiempo
            Invoke(nameof(DisableHitbox), 0.3f);
        }

        // Opcional: si quieres dañar directamente a objetos al golpear desde arriba
        IDamageable damageable = collision.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            
            ContactPoint contact = collision.contacts[0];
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                areaHitboxOnImpact.SetDamage(25f, 8f, 3f);
                areaHitboxOnImpact.EnableHitbox();

                // Desactivamos el hitbox después de un tiempo
                Invoke(nameof(DisableHitbox), 0.3f);
            }
        }
    }

    private void DisableHitbox()
    {
        areaHitboxOnImpact.DisableHitbox();
        //Destroy(gameObject); // si quieres eliminar el cubo tras el impacto
    }
}