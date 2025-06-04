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
    public void OnDespawn() { }

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
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
            poolManager?.Release(this);
        }
    }

    /// <summary>
    /// Initializes projectile velocity and damage.
    /// </summary>
    public void Initialize(Vector3 velocity, float damageAmount, PoolManager<Projectile> pool)
    {
        rb.velocity = velocity;
        damage = damageAmount;
        poolManager = pool;
    }
}

