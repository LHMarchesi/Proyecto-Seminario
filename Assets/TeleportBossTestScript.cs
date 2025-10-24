using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBossTestScript : MonoBehaviour
{
    [Header("General Settings")]
    public Transform player;
    public float attackInterval = 3f;
    public float projectileSpeed = 20f;
    public float levitationAmplitude = 0.5f;
    public float levitationSpeed = 2f;
    private Vector3 initialPosition;
    private bool isAttacking = false;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public GameObject spawnPrefab;
    public float rangeRadius = 15f;
    public float minTeleportHeight = 2f;
    public int poolSize = 30;
    private Queue<GameObject> projectilePool;

    [Header("Attack B Settings")]
    public int burstCount = 10;
    public float burstDelay = 0.1f;
    public float postTeleportDelay = 1.5f;

    [Header("Attack C Settings")]
    public GameObject fallingProjectilePrefab;
    public float fallingSpawnHeight = 15f;
    public float fallingSpeed = 40f;

    void Start()
    {
        initialPosition = transform.position;

        // Buscar player automáticamente
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Boss no encontró un objeto con tag 'Player'.");

        InitializeProjectilePool();
        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        FacePlayer();
        Levitate();
    }

    // --- 🔹 Levitación flotante ---
    private void Levitate()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * levitationSpeed) * levitationAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // --- 🔹 Siempre mirar al jugador ---
    private void FacePlayer()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    // --- 🔹 Pooling básico ---
    private void InitializeProjectilePool()
    {
        projectilePool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject proj = Instantiate(projectilePrefab);
            proj.SetActive(false);
            projectilePool.Enqueue(proj);
        }
    }

    private GameObject GetProjectileFromPool()
    {
        if (projectilePool.Count > 0)
        {
            GameObject proj = projectilePool.Dequeue();
            proj.SetActive(true);
            return proj;
        }
        else
        {
            return Instantiate(projectilePrefab);
        }
    }

    private void ReturnProjectileToPool(GameObject proj)
    {
        proj.SetActive(false);
        projectilePool.Enqueue(proj);
    }

    // --- 🔹 Ciclo de ataques ---
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!isAttacking)
            {
                int attackType = Random.Range(0, 3); // A, B o C
                switch (attackType)
                {
                    case 0: yield return StartCoroutine(AttackA()); break;
                    case 1: yield return StartCoroutine(AttackB()); break;
                    case 2: yield return StartCoroutine(AttackC()); break;
                }
                yield return new WaitForSeconds(attackInterval);
            }
            yield return null;
        }
    }

    // --- 🌀 ATAQUE A: Spawnear 3 prefabs en puntos aleatorios dentro del rango ---
    private IEnumerator AttackA()
    {
        isAttacking = true;

        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPoint = GetRandomPointInRange();
            Instantiate(spawnPrefab, randomPoint, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }

        isAttacking = false;
    }

    // --- ⚡ ATAQUE B: Teletransportarse, esperar, disparar ráfaga ---
    private IEnumerator AttackB()
    {
        isAttacking = true;

        Vector3 randomPoint = GetRandomPointInRange();
        transform.position = randomPoint;
        yield return new WaitForSeconds(postTeleportDelay);

        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;
            GameObject proj = GetProjectileFromPool();
            proj.transform.position = transform.position + transform.forward * 2f;
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                rb.velocity = dir * projectileSpeed;
            }
            StartCoroutine(DeactivateAfterSeconds(proj, 5f));
            yield return new WaitForSeconds(burstDelay);
        }

        isAttacking = false;
    }

    // --- 💥 ATAQUE C: Teletransportar al jugador y lanzar proyectil desde arriba ---
    private IEnumerator AttackC()
    {
        isAttacking = true;

        if (player == null)
        {
            isAttacking = false;
            yield break;
        }

        Vector3 randomPoint = GetRandomPointInRange();
        player.position = randomPoint;

        Vector3 spawnPos = randomPoint + Vector3.up * fallingSpawnHeight;
        GameObject fallingProj = Instantiate(fallingProjectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = fallingProj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = Vector3.down * fallingSpeed;

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // --- Utilidades ---
    private Vector3 GetRandomPointInRange()
    {
        Vector2 randomCircle = Random.insideUnitCircle * rangeRadius;

        // 🔹 Usamos la posición actual del jefe como centro, no la inicial
        Vector3 basePosition = transform.position;

        // 🔹 Creamos el punto con Y = posición actual (o la altura mínima, lo que sea mayor)
        float targetY = Mathf.Max(basePosition.y, minTeleportHeight);

        Vector3 randomPoint = new Vector3(
            basePosition.x + randomCircle.x,
            targetY,
            basePosition.z + randomCircle.y
        );

        return randomPoint;
    }

    private IEnumerator DeactivateAfterSeconds(GameObject proj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (proj != null && proj.activeSelf)
            ReturnProjectileToPool(proj);
    }

    // --- Gizmos para ver el rango ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangeRadius);
    }
}
