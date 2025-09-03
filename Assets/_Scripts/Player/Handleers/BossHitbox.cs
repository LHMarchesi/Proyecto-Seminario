using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossHitbox : MonoBehaviour
{
    private float damage;
    private float knockbackHorizontal;
    private float knockbackVertical;

    private Collider hitboxCollider;
    private MeshRenderer renderer;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        renderer = GetComponent<MeshRenderer>();
        hitboxCollider.isTrigger = true;
        DisableHitbox();
    }

    public void SetDamage(float dmg, float knockbackHorizontal = 0f, float knockbackVertical = 0f)
    {
        damage = dmg;
        this.knockbackHorizontal = knockbackHorizontal;
        this.knockbackVertical = knockbackVertical;
    }

    public void EnableHitbox() { hitboxCollider.enabled = true; renderer.enabled = true; }
    public void DisableHitbox() { hitboxCollider.enabled = false; renderer.enabled = false; }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable dmgTarget = other.GetComponent<IDamageable>();
        if (dmgTarget != null)
        {
            dmgTarget.TakeDamage(damage);

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                ApplyKnockback(other);
            }
        }
    }

    private void ApplyKnockback(Collider other)
    {
        if (knockbackHorizontal <= 0f && knockbackVertical <= 0f) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (other.transform.position - transform.position);
            dir.y = 0;

            // Evita vector cero
            if (dir.sqrMagnitude < 0.01f)
                dir = transform.forward; // empuja hacia adelante del boss si están muy cerca

            dir.Normalize();

            // Aplica impulso
            Vector3 force = dir * knockbackHorizontal + Vector3.up * knockbackVertical;
            rb.AddForce(force, ForceMode.VelocityChange);
        }
    }
}

