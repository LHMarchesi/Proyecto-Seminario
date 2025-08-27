using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class BossManagerTest : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Referencias")]
    [Tooltip("Jugador a apuntar. Si está vacío, se usa el forward del jefe.")]
    public Transform player;
    [Tooltip("Punto de salida de proyectiles (opcional).")]
    public Transform shootPoint;
    [Tooltip("Prefab del proyectil con Rigidbody + Collider.")]
    public GameObject projectilePrefab;
    [Tooltip("Prefab del pincho (con collider para dañar).")]
    public GameObject spikePrefab;

    [Header("Control de Ataques")]
    [Tooltip("Tiempo entre ataques (segundos).")]
    public float attackPeriod = 3f;

    [Header("Ataque A — Proyectiles en cono")]
    public int projectileCount = 3;
    public float spreadAngle = 15f;
    public float projectileSpeed = 12f;

    [Header("Ataque B — Teleport + Dash (alrededor del jefe)")]
    public float teleportRadius = 12f;
    public float delayBeforeDash = 0.25f;
    public float dashForwardDistance = 8f;
    public float dashForwardDuration = 0.25f;
    public bool alignToGround = true;
    public LayerMask groundMask = ~0;      
    public float groundCheckHeight = 5f;   

    [Header("Ataque C — Pinchos bajo el jugador")]
    [Tooltip("Retraso antes de que salgan los pinchos.")]
    public float spikeDelay = 1.0f;
    [Tooltip("Altura inicial (debajo del suelo).")]
    public float spikeStartYOffset = -1.0f;
    [Tooltip("Altura que suben al emerger.")]
    public float spikeRiseHeight = 1.5f;
    [Tooltip("Tiempo que tardan en subir.")]
    public float spikeRiseDuration = 0.2f;
    [Tooltip("Segundos antes de destruir el pincho.")]
    public float spikeLifetime = 3f;

    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        if (shootPoint == null) shootPoint = transform; // fallback
    }

    void OnEnable()
    {
        StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    // ======================
    // LOOP: Atacar cada X s
    // ======================
    IEnumerator AttackLoop()
    {
        // Espera inicial (opcional)
        yield return new WaitForSeconds(attackPeriod);

        while (!isDead)
        {
            int pick = Random.Range(0, 3);
            switch (pick)
            {
                case 0: yield return StartCoroutine(AttackA_ProjectileCone()); break;
                case 1: yield return StartCoroutine(AttackB_TeleportDash()); break;
                case 2: yield return StartCoroutine(AttackC_Spikes()); break;
            }

            // Espera entre ataques (no se solapan)
            yield return new WaitForSeconds(attackPeriod);
        }
    }

    // ==========================
    // ATAQUE A: Proyectiles cono
    // ==========================
    IEnumerator AttackA_ProjectileCone()
    {
        Vector3 originPos = shootPoint.position;

        // Dirección base: hacia el jugador si existe, sino forward del jefe
        Vector3 dir = (player != null)
            ? (player.position - originPos).normalized
            : shootPoint.forward;

        Quaternion baseRot = Quaternion.LookRotation(dir, Vector3.up);

        for (int i = 0; i < projectileCount; i++)
        {
            float offset = (i - (projectileCount - 1) * 0.5f) * spreadAngle;
            Quaternion rot = Quaternion.AngleAxis(offset, Vector3.up) * baseRot;

            GameObject proj = Instantiate(projectilePrefab, originPos, rot);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = rot * Vector3.forward * projectileSpeed;
            }
        }

        // Pequeña pausa opcional para “ritmo” (no necesaria)
        yield return null;
    }

    // ===========================
    // ATAQUE B: Teleport + Dash
    // ===========================
    IEnumerator AttackB_TeleportDash()
    {
        // 1) Elegir un punto al azar alrededor del JEFE (no del jugador)
        Vector3 origin = transform.position;
        Vector2 r = Random.insideUnitCircle;
        // distancia aleatoria dentro del radio (no siempre al borde)
        float dist = Random.Range(0.25f * teleportRadius, teleportRadius);
        Vector3 tpPos = new Vector3(origin.x + r.normalized.x * dist,
                                    origin.y,
                                    origin.z + r.normalized.y * dist);

        // Opcional: alinear con el suelo para evitar quedar flotando/enterrado
        if (alignToGround)
        {
            Vector3 from = tpPos + Vector3.up * groundCheckHeight;
            if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, groundCheckHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                tpPos.y = hit.point.y;
            }
        }

        // 2) Teletransportarse a ese punto
        transform.position = tpPos;

        // 3) Espera breve y dash HACIA ADELANTE (forward actual del jefe)
        yield return new WaitForSeconds(delayBeforeDash);

        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * dashForwardDistance;

        float t = 0f;
        while (t < dashForwardDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / dashForwardDuration);
            transform.position = Vector3.Lerp(start, end, a);
            yield return null;
        }
    }

    // ======================================
    // ATAQUE C: Pinchos bajo el jugador
    // ======================================
    IEnumerator AttackC_Spikes()
    {
        // Si no hay jugador, los genera al frente del jefe
        Vector3 basePos = (player != null) ? player.position : (transform.position + transform.forward * 1.5f);
        Vector3 startPos = new Vector3(basePos.x, basePos.y + spikeStartYOffset, basePos.z);

        GameObject spike = Instantiate(spikePrefab, startPos, Quaternion.identity);

        // Espera antes de emerger
        yield return new WaitForSeconds(spikeDelay);

        // Subida
        Vector3 endPos = startPos + Vector3.up * spikeRiseHeight;
        float t = 0f;
        while (t < spikeRiseDuration && spike != null)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / spikeRiseDuration);
            spike.transform.position = Vector3.Lerp(startPos, endPos, a);
            yield return null;
        }

        // Auto-destruir tras su vida útil
        if (spike != null) Destroy(spike, spikeLifetime);
    }

    // ======================
    // SISTEMA DE DAÑO/VIDA
    // ======================
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 c = (player != null) ? player.position : transform.position;
        Gizmos.DrawWireSphere(c, teleportRadius);
    }
#endif
}
