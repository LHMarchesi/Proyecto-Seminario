using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class HandleAttack : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerContext playerContext;

    [Header("Attacking")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackSpeed;
    [SerializeField] public int attackDamage;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;


    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;
    private float playerSpeed;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
        if (playerContext.HandleInputs.IsAttacking())
            Attack();
    }

    public void Attack() // Attack using forward RayCast
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);  // Play swing
        audioSource.PlayOneShot(swordSwing);

        playerSpeed = playerContext.PlayerController.currentSpeed;
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.currentSpeed / 2); // Reduce speed while attacking
    }

    void ResetAttack()
    {
        playerContext.PlayerController.ChangeSpeed(playerSpeed); // Restore speed
        attacking = false;
        readyToAttack = true;
    }


    void AttackRaycast()
    {
        Vector3 origin = Camera.main.transform.position + Camera.main.transform.forward * (attackDistance * 0.5f);

        Collider[] hits = Physics.OverlapSphere(origin, 2.5f, attackLayer); // radio de 1 metro
        foreach (var hit in hits)
        {
            HitTarget(hit.ClosestPoint(origin));
            IDamageable damagable = hit.GetComponent<IDamageable>();
            if (damagable != null)
            {
                damagable.TakeDamage(attackDamage);

                StartCoroutine(HitStop(0.08f, hit.gameObject));
                StartCoroutine(ScreenShake(0.1f, 0.10f));
            }
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity); // Instantiate effect
        Destroy(GO, 3);
    }

    private IEnumerator HitStop(float duration, GameObject enemy)
    {
        float originalPlayerSpeed = playerContext.PlayerController.currentSpeed;
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();

        playerContext.PlayerController.ChangeSpeed(0);

        Vector3 enemyVel = Vector3.zero;
        if (enemyRb != null)
        {
            enemyVel = enemyRb.velocity;
            enemyRb.isKinematic = true;
        }

        yield return new WaitForSecondsRealtime(duration);

        playerContext.PlayerController.ChangeSpeed(originalPlayerSpeed);
        if (enemyRb != null)
        {
            enemyRb.isKinematic = false;
            enemyRb.velocity = enemyVel;
        }
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}



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

public class BossEnemy : BaseEnemy
{
    private enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Damaged
    }

    [Header("Boss Attacks by Phase")]
    public BossAttackStats[] phase1Attacks;
    public BossAttackStats[] phase2Attacks;
    public BossAttackStats[] phase3Attacks;

    [SerializeField] private BossHitbox meleeHitBox;

    private BossState currentState;
    private int currentPhase = 1;
    private float attackCooldown;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = BossState.Idle;
        attackCooldown = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        if (target == null) return;

        HandlePhases();

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case BossState.Idle:
                handleAnimations.ChangeAnimationState("Idle_Boss");
                if (distance < stats.detectionRange)
                    currentState = BossState.Chasing;
                break;

            case BossState.Chasing:
                if (distance > stats.detectionRange)
                    currentState = BossState.Idle;
                else if (distance <= GetCurrentAttackRange())
                    currentState = BossState.Attacking;
                else
                    ChaseTarget();
                break;

            case BossState.Attacking:
                if (distance > GetCurrentAttackRange())
                {
                    currentState = BossState.Chasing;
                }
                else
                {
                    PerformAttack();
                }
                break;

            case BossState.Damaged:
                // el da�o ya puede tener animaci�n de stun o knockback
                break;
        }
    }

    private void HandlePhases()
    {
        float healthPercent = currentHealth / stats.maxHealth;

        if (healthPercent <= 0.2f && currentPhase < 3)
            currentPhase = 3;
        else if (healthPercent <= 0.5f && currentPhase < 2)
            currentPhase = 2;
    }

    private void ChaseTarget()
    {
        handleAnimations.ChangeAnimationState("Chasing_Boss");
        MoveTowardsTarget();
        FaceTarget();
    }

    private void PerformAttack()
    {
        if (attackCooldown > 0f) return;

        BossAttackStats[] attacks = GetAttacksForPhase();
        if (attacks == null || attacks.Length == 0) return;

        BossAttackStats chosenAttack = null;

        // Intentamos varios ataques hasta encontrar uno v�lido
        for (int i = 0; i < 5; i++)
        {
            BossAttackStats candidate = attacks[Random.Range(0, attacks.Length)];
            if (candidate.IsInRange(transform, target))
            {
                chosenAttack = candidate;
                break;
            }
        }

        if (chosenAttack == null)
        {
            currentState = BossState.Chasing; // no encontr� ataque, sigue persiguiendo
            return;
        }

        // Ejecutar el ataque
        chosenAttack.Execute(this, target, handleAnimations);
        attackCooldown = chosenAttack.cooldown; // usar el cooldown del ataque
    }

    private float GetCurrentAttackRange()
    {
        BossAttackStats[] attacks = GetAttacksForPhase();
        float maxRange = 0f;

        foreach (var attack in attacks)
        {
            if (attack.range > maxRange)
                maxRange = attack.range;
        }

        return maxRange;
    }

    private BossAttackStats[] GetAttacksForPhase()
    {
        if (currentPhase == 1) return phase1Attacks;
        if (currentPhase == 2) return phase2Attacks;
        if (currentPhase == 3) return phase3Attacks;
        return null;
    }

    protected override void OnDamage(float damage)
    {
        Invoke(nameof(EndDamageState), 0.5f);
        base.OnDamage(damage);
        handleAnimations.ChangeAnimationState("TakeDamage_Boss");
        GetKnockback(stats.knockbackAmmount);

        currentState = BossState.Damaged;
    }

    private void EndDamageState()
    {
        currentState = BossState.Chasing;
    }

    public void PrepareAttack(float damage)
    {
        if (meleeHitBox != null)
            meleeHitBox.SetDamage(damage);
    }
}

public abstract class BossAttackStats : ScriptableObject
{
    public string attackName;
    public float cooldown;
    public float damage;
    public float range;

    private float lastAttackTime = -Mathf.Infinity;

    protected bool CanExecute()
    {
        return Time.time >= lastAttackTime + cooldown;
    }
    protected void MarkExecuted()
    {
        lastAttackTime = Time.time;
    }
    public abstract void Execute(BossEnemy boss, Transform target, HandleAnimations animations);

    public bool IsInRange(Transform boss, Transform target)
    {
        float dist = Vector3.Distance(boss.position, target.position);
        return dist <= range;
    }
}


public class MeleeBossAttack : BossAttackStats
{
    public string animationName;
    public float attackDelay; // delay before hitbox activates
    public float attackDuration; // duration of hitbox active
    public override void Execute(BossEnemy boss, Transform target, HandleAnimations animations)
    {
        if (!CanExecute()) return;
        animations.ChangeAnimationState(animationName);
        boss.PrepareAttack(damage);
        // Activar hitbox despu�s del delay
        boss.Invoke(nameof(BossHitbox.EnableHitbox), attackDelay);
        // Desactivar hitbox despu�s de la duraci�n
        boss.Invoke(nameof(BossHitbox.DisableHitbox), attackDelay + attackDuration);
        MarkExecuted();
    }
}
