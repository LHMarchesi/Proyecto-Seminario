using UnityEngine;

public class DragonBoss : MonoBehaviour, IDamageable
{
    private float currentHealth;
    [SerializeField] private BossState currentState;
    private int currentPhase = 1;
    private HandleAnimations handleAnimations;
    private Transform target;
    [SerializeField] private float maxHealth;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float rangeAttackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float rangeAttackDamage;
    [SerializeField] private float currentRangeAttackCooldown;

    private enum BossState
    {
        Entry,
        Idle,
        Attacking,
        Damaged
    }
    public void TakeDamage(float damage)
    {
    }

    public void Start()
    {
        handleAnimations = GetComponent<HandleAnimations>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = BossState.Idle;
        currentRangeAttackCooldown = attackCooldown;
    }

    private void Update()
    {
        if (currentRangeAttackCooldown > 0f)
            currentRangeAttackCooldown -= Time.deltaTime;



        if (target == null) return;

        HandlePhases();

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case BossState.Entry:
                // handleAnimations.ChangeAnimationState("Entry_Boss");
                // After entry animation ends, switch to Idle
                currentState = BossState.Idle;
                break;


            case BossState.Idle:
                //  handleAnimations.ChangeAnimationState("Idle_Boss");
                if (distance < rangeAttackRange)
                    currentState = BossState.Attacking;
                break;

            case BossState.Attacking:
                if (distance > rangeAttackRange)
                {
                    currentState = BossState.Idle;
                }
                else
                {
                    if (currentRangeAttackCooldown < 0f)
                    {
                        handleAnimations.ChangeAnimationState("RangeAttack_Boss", true);
                        Debug.Log("Dragon Boss performs range attack");
                        currentRangeAttackCooldown = attackCooldown;
                        currentState = BossState.Idle;
                    }
                }
                break;

            case BossState.Damaged:

                break;
        }
    }

    public void PerformRangeAttack()
    {
        Vector3 directionToTarget = (target.position - firePoint.position).normalized;

        GameObject project = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionToTarget));
        Projectile projectileScript = project.GetComponent<Projectile>();
        Rigidbody projectilRB = project.GetComponent<Rigidbody>();
        projectilRB.velocity = directionToTarget * projectileSpeed;
        projectileScript.SetDamage(rangeAttackDamage);
    }

    private void HandlePhases()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= 0.2f && currentPhase < 3)
            currentPhase = 3;
        else if (healthPercent <= 0.5f && currentPhase < 2)
            currentPhase = 2;
    }

}
