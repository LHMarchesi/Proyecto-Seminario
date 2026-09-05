using System;
using TMPro;
using UnityEngine;

public enum DamageFeedbackType
{
    Normal,
    Poison,
    Electricity,
    Fire
}

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{

    [SerializeField] public EnemyStats baseStats;
    [SerializeField] public EnemyStats currentStats;
    [Header("VFX")]
    [Header("Combat VFX")]
    [SerializeField]
    private Transform combatVFXAnchor;

    public Transform CombatVFXAnchor =>
        combatVFXAnchor;

    public Vector3 CombatVFXPosition =>
        combatVFXAnchor != null
            ? combatVFXAnchor.position
            : transform.position + Vector3.up;

    [Header("Damage Feedback")]
    [SerializeField] private GameObject floatingTextPrefab;

    [SerializeField]
    private Vector3 damageTextOffset =
        new Vector3(0f, 0.5f, 0f);

    [Header("Damage Colors")]
    [SerializeField]
    private Color normalDamageColor =
        Color.white;

    [SerializeField]
    private Color poisonDamageColor =
        new Color(0.3f, 1f, 0.3f);

    [SerializeField]
    private Color electricityDamageColor =
        new Color(0.3f, 0.8f, 1f);

    [SerializeField]
    private Color fireDamageColor =
        new Color(1f, 0.4f, 0.1f);

    [Header("Multiple Damage Numbers")]
    [SerializeField] private float popupClusterResetTime = 0.15f;
    [SerializeField] private float popupHorizontalSpacing = 0.20f;
    [SerializeField] private float popupVerticalSpacing = 0.12f;

    private float lastDamagePopupTime;
    private int popupClusterIndex;

    [Header("Runtime Stats (copiados del template)")]
    public float maxHealth;
    public float moveSpeed;
    public float attackDamage;
    public float attackSpeed;
    public float expDrop;

    private float damageCooldown = 0.2f; // medio segundo de invulnerabilidad
    private float lastDamageTime = -Mathf.Infinity;

    [SerializeField] protected float currentHealth;
    public float CurrentHealth => currentHealth;

    protected Transform target;
    protected Vector3 spawnPosition;
    protected HandleAnimations handleAnimations;
    protected Rigidbody rb;
    protected ExperienceManager playerEXP;

    public Action OnDeath;
    private EnemySpawner spawner;

    protected virtual void OnEnable()
    {
        handleAnimations = GetComponent<HandleAnimations>();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();

        if (player != null)
            target = player.transform;
    }

    public virtual void Initialize(EnemySpawner spawner = null)
    {
        currentStats = ScriptableObject.Instantiate(baseStats); 

        this.spawner = spawner;
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();


        //maxHealth = currentStats.maxHealth;
        //moveSpeed = currentStats.moveSpeed;
        //attackDamage = currentStats.attackDamage;
        //attackSpeed = currentStats.attackSpeed;
        //expDrop = currentStats.expDrop;

        currentHealth = maxHealth;


        var flock = GetComponent<FlockingBehave>();
        if (flock != null && spawner != null)
        {
            flock.Initialize(spawner, spawner.cohesionWeight, spawner.separationWeight, spawner.alignmentWeight, spawner.neighborRadius);
        }


    }

    public void ApplyDifficulty(float difficulty)
    {
        Debug.Log("Se Aplico dificultad");

        attackDamage += 2f * difficulty;
        maxHealth += 5f * difficulty;
        moveSpeed += 0.05f * difficulty;

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
    }
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void TakeDamage(float damage, DamageFeedbackType feedbackType = DamageFeedbackType.Normal)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;
        OnDamage(damage, feedbackType);
    }

    public void TakeEffectDamage(
     float damage,
     DamageFeedbackType feedbackType =
         DamageFeedbackType.Normal)
    {
        if (damage <= 0f ||
            IsDead())
            return;

        currentHealth -= damage;

        ShowDamageNumber(
            damage,
            feedbackType
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    public void ShowDamageNumber(
      float damage,
      DamageFeedbackType feedbackType =
          DamageFeedbackType.Normal)
    {
        if (floatingTextPrefab == null)
            return;

        // ==========================================
        // EVITAR QUE VARIOS NUMEROS SE SUPERPONGAN
        // ==========================================

        if (Time.time - lastDamagePopupTime >
            popupClusterResetTime)
        {
            popupClusterIndex = 0;
        }
        else
        {
            popupClusterIndex++;
        }

        lastDamagePopupTime = Time.time;

        Vector3 popupOffset =
            GetDamageNumberOffset(
                popupClusterIndex
            );

        Vector3 spawnPosition =
            CombatVFXPosition +
            damageTextOffset +
            popupOffset;

        // No lo parentamos al enemigo.
        // Así el número sigue visible si el daño mata
        // al enemigo inmediatamente.
        GameObject go =
            Instantiate(
                floatingTextPrefab,
                spawnPosition,
                Quaternion.identity
            );

        TextMeshPro textMesh =
            go.GetComponent<TextMeshPro>();

        if (textMesh != null)
        {
            textMesh.text =
                Mathf.CeilToInt(damage)
                .ToString();

            textMesh.color =
                GetDamageFeedbackColor(
                    feedbackType
                );
        }

        // Safety por si el prefab no se destruye solo.
        Destroy(go, 2f);
    }

    private Color GetDamageFeedbackColor(
    DamageFeedbackType type)
    {
        switch (type)
        {
            case DamageFeedbackType.Poison:
                return poisonDamageColor;

            case DamageFeedbackType.Electricity:
                return electricityDamageColor;

            case DamageFeedbackType.Fire:
                return fireDamageColor;

            default:
                return normalDamageColor;
        }
    }

    private Vector3 GetDamageNumberOffset(
    int index)
    {
        int slot = index % 5;

        switch (slot)
        {
            case 1:
                return new Vector3(
                    -popupHorizontalSpacing,
                    popupVerticalSpacing,
                    0f
                );

            case 2:
                return new Vector3(
                    popupHorizontalSpacing,
                    popupVerticalSpacing * 2f,
                    0f
                );

            case 3:
                return new Vector3(
                    -popupHorizontalSpacing * 1.5f,
                    popupVerticalSpacing * 3f,
                    0f
                );

            case 4:
                return new Vector3(
                    popupHorizontalSpacing * 1.5f,
                    popupVerticalSpacing * 4f,
                    0f
                );

            default:
                return Vector3.zero;
        }
    }

    public void ApplyExternalKnockback(Vector3 direction, float force, float upwardForce = 0f)
    {
        if (rb == null || force <= 0f)
            return;

        Vector3 finalDirection = direction.normalized;
        finalDirection.y = 0f;

        rb.AddForce(finalDirection * force, ForceMode.Impulse);

        if (upwardForce > 0f)
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
    }

    protected virtual void OnDamage(float damage, DamageFeedbackType feedbackType)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }
    protected virtual void Attack()
    {
    }

    protected virtual void Die(float experienceDroped = 0)
    {
        if (spawner != null)
            spawner.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);

        OnDeath?.Invoke();
        //playerEXP.AddExperience(experienceDroped);
        GetComponent<EnemyDropManager>()?.DropItems();
    }

    public virtual void Spawn(Transform spawnPos)
    {
        currentHealth = baseStats.maxHealth;
        transform.position = spawnPos.position;
        gameObject.SetActive(true); // Para pooling
    }

    protected void FaceTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.forward = direction;
    }

    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.Translate(direction * baseStats.moveSpeed * Time.deltaTime, Space.World);
    }

    protected void GetKnockback(float knockbackAmount)
    {
        rb.AddForce((transform.position - target.position).normalized * knockbackAmount, ForceMode.Impulse);
        rb.AddForce(Vector3.up * knockbackAmount, ForceMode.Impulse);
    }
}
