using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 2f;
    private float timer;
    private float damage;
    private Rigidbody rb;
    private PoolManager<Projectile> poolManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Called by PoolManager when spawning.
    /// </summary>
    public void OnSpawn()
    {
        timer = 0f;
    }

    /// <summary>
    /// Called by PoolManager when despawning.
    /// </summary>
    public void OnDespawn() { 
        rb.velocity = Vector3.zero; // Reset velocity when despawning
        poolManager = null; // Clear reference to PoolManager
        timer = 0f; // Reset timer
        // Deactivate the game object
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            poolManager?.Release(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Obtener el script del Player y aplicarle daño
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.TakeDamage(damage);
        }
        poolManager?.Release(this);
        //gameObject.SetActive(false);
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    /// <summary>
    /// Initializes projectile velocity and damage.
    /// </summary>
    public void Initialize(Vector3 velocity, float damageAmount, PoolManager<Projectile> pool)
    {
        gameObject.SetActive(true); // Activate the game object
        rb.velocity = velocity;
        damage = damageAmount;
        poolManager = pool;
    }
}

