using UnityEngine;

public class RangeEnemy : BaseEnemy
{
    private enum RangeEnemyState
    {
        Idle,
        Chasing,
        Patrolling,
        Attacking,
        Damaged
    }

    private RangeEnemyState currentState;
    private float attackCooldown;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = RangeEnemyState.Idle;
        attackCooldown = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case RangeEnemyState.Idle:
                handleAnimations.ChangeAnimationState("Idle_RangeEnemy");
                if (distance < stats.detectionRange)
                    currentState = RangeEnemyState.Chasing;
                break;

            case RangeEnemyState.Chasing:
                if (distance > stats.detectionRange)
                {
                    currentState = RangeEnemyState.Idle;
                }
                else if (distance <= stats.attackRange)
                {
                    currentState = RangeEnemyState.Attacking;
                }
                else
                    HoverAbovePlayer();
                break;

            case RangeEnemyState.Attacking:
                if (distance > stats.attackRange)
                    currentState = RangeEnemyState.Chasing;
                else
                    Attack();
                break;
            case RangeEnemyState.Damaged:
                break;
        }
    }


    protected override void OnDamage(float damage)
    {
        base.OnDamage(damage);
        handleAnimations.ChangeAnimationState("TakeDamage_RangeEnemy");
        currentState = RangeEnemyState.Damaged;
        Invoke(nameof(EndDamageState), 0.3f);
    }

    private void EndDamageState()
    {
        currentState = RangeEnemyState.Chasing;
    }

    private void HoverAbovePlayer()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + Vector3.up * 4f; // Altura sobre el jugador
        desiredPosition.y = Mathf.Lerp(transform.position.y, desiredPosition.y, Time.deltaTime * 2f); // Suaviza la transición

        Vector3 horizontalPosition = new Vector3(
            Vector3.MoveTowards(transform.position, target.position, stats.moveSpeed * Time.deltaTime).x,
            desiredPosition.y,
            Vector3.MoveTowards(transform.position, target.position, stats.moveSpeed * Time.deltaTime).z
        );

        transform.position = horizontalPosition;
        FaceTarget();
    }

    private void Attack() //Shoot
    {
        Debug.Log("Shot");
        /*
        if (attackCooldown > 0f) return;

        handleAnimations.ChangeAnimationState("Attack_RangeEnemy");
        //audioSource.PlayOneShot(swordSwing);
        attackCooldown = 1f / stats.attackSpeed;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 direction = (target.position - transform.position).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, stats.attackRange))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(stats.attackDamage);
        }
        */
    }
}
