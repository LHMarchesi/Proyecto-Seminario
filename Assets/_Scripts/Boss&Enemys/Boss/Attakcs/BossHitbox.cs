using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    private float damage;      

    private void Awake()
    {
        GetComponent<Collider>().enabled = false; // desactivada al inicio
    }

    private void OnTriggerEnter(Collider other)
    {
            IDamageable dmg = other.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                Debug.Log("Player recibi� " + damage + " de da�o con la patada!");
            }
    }

    public void SetDamage(float damage) { this.damage = damage; }

    // Llamado por animaci�n o por el Boss
    public void EnableHitbox() => GetComponent<Collider>().enabled = true;
    public void DisableHitbox() => GetComponent<Collider>().enabled = false;
}