using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float entryScene_Range;
    [SerializeField] private float rangeAttack_Range;
    [SerializeField] private float rangeAttack_Cooldown;
    [SerializeField] private float rangeAttack_Damage;
    [SerializeField] private float rangeAttack_CurrentCooldown;
    SkinnedMeshRenderer bossRenderer;

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
        bossRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        bossRenderer.gameObject.SetActive(false);

        currentState = BossState.Entry;
        rangeAttack_CurrentCooldown = rangeAttack_Cooldown;

        projectilePoolManager = new PoolManager<Projectile>(projectilePrefab.GetComponent<Projectile>(), poolSize, transform);
    }

    private void Update()
    {
        if (rangeAttack_CurrentCooldown > 0f)
            rangeAttack_CurrentCooldown -= Time.deltaTime;

        if (target == null) return;

        HandlePhases();

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case BossState.Entry:
                if (distance < entryScene_Range)
                {
                    handleAnimations.ChangeAnimationState("Entry_Boss");
                    bossRenderer.gameObject.SetActive(true);
                    StartCoroutine(WaitForEntryAnimation());
                }
                break;


            case BossState.Idle:
                handleAnimations.ChangeAnimationState("Idle_Boss");
                if (distance < rangeAttack_Range)
                    currentState = BossState.Attacking;
                break;

            case BossState.Attacking:
                if (distance > rangeAttack_Range)
                {
                    currentState = BossState.Idle;
                }
                else
                {
                    if (rangeAttack_CurrentCooldown < 0f)
                    {
                        handleAnimations.ChangeAnimationState("RangeAttack_Boss", true);
                        Debug.Log("Dragon Boss performs range attack");
                        rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
                    }
                }
                break;

            case BossState.Damaged:

                break;
        }
    }

    private IEnumerator WaitForEntryAnimation()
    {
        yield return null; // Espera un frame para asegurar que la animación empezó
        float animLength = handleAnimations.GetCurrentAnimationLength();
        Debug.Log("waiting for entry animation: " + animLength);
        yield return new WaitForSeconds(animLength);
        Debug.Log("entry animation finished");
        currentState = BossState.Idle;
    }

    PoolManager<Projectile> projectilePoolManager;
    private int poolSize = 7;
    public void PerformRangeAttack() // Usado en animation event instancia un proyectil
    {
        Vector3 directionToTarget = (target.position - firePoint.position).normalized;

        GameObject projectile = projectilePoolManager.Get().gameObject;
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.Initialize(directionToTarget * projectileSpeed, rangeAttack_Damage, projectilePoolManager, firePoint.transform);
    }


    private void HandlePhases()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= 0.2f && currentPhase < 3)
            currentPhase = 3;
        else if (healthPercent <= 0.5f && currentPhase < 2)
            currentPhase = 2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeAttack_Range);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, entryScene_Range);
    }

}
