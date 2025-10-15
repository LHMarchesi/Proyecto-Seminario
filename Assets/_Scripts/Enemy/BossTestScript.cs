using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTestScript : MonoBehaviour
{
    [Header("General Settings")]
    public Transform player;
    public GameObject projectilePrefab;
    public float attackInterval = 3f;       // 🔹 Nuevo: tiempo de espera real entre ataques
    public float projectileSpeed = 10f;

    [Header("Pooling Settings")]
    public int poolSize = 50;
    private Queue<GameObject> projectilePool;

    [Header("Attack A - Escopeta")]
    public int pellets = 3;
    public float spreadAngle = 20f;

    [Header("Attack B - Bola sobre el jugador")]
    public GameObject delayedProjectilePrefab;
    public float spawnDelay = 1f;
    public float attackBHeightOffset = 0.5f;

    [Header("Attack C y D - Ráfaga tipo estrella")]
    public int starProjectiles = 5;
    public float starDuration = 3f;
    public float starFireRate = 0.2f;
    public float starRotationSpeed = 90f;

    private bool isAttacking = false;

    void Start()
    {
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
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    // --- Pooling ---
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
            GameObject newProj = Instantiate(projectilePrefab);
            return newProj;
        }
    }

    private void ReturnProjectileToPool(GameObject proj)
    {
        proj.SetActive(false);
        projectilePool.Enqueue(proj);
    }

    // --- Ciclo de ataques con intervalo real ---
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!isAttacking)
            {
                int attackType = Random.Range(0, 4);
                switch (attackType)
                {
                    case 0: yield return StartCoroutine(AttackA()); break;
                    case 1: yield return StartCoroutine(AttackB()); break;
                    case 2: yield return StartCoroutine(AttackC()); break;
                    case 3: yield return StartCoroutine(AttackD()); break;
                }

                // 🔹 Esperar un tiempo fijo antes de iniciar el siguiente ataque
                yield return new WaitForSeconds(attackInterval);
            }
            yield return null;
        }
    }

    // --- Ataques ---
    private IEnumerator AttackA()
    {
        if (player == null) yield break;
        isAttacking = true;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        for (int i = 0; i < pellets; i++)
        {
            float angleOffset = ((float)i - (pellets - 1) / 2f) * spreadAngle;
            Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.up);
            Vector3 shootDir = rotation * dirToPlayer;

            GameObject proj = GetProjectileFromPool();
            proj.transform.position = transform.position + shootDir;
            proj.transform.rotation = Quaternion.LookRotation(shootDir);

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = shootDir * projectileSpeed;

            StartCoroutine(DeactivateAfterSeconds(proj, 5f));
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private IEnumerator AttackB()
    {
        if (player == null) yield break;
        isAttacking = true;

        Vector3 spawnPos = player.position + Vector3.up * attackBHeightOffset;
        yield return new WaitForSeconds(spawnDelay);

        GameObject proj = GetProjectileFromPool();
        proj.transform.position = spawnPos;
        proj.transform.rotation = Quaternion.identity;

        StartCoroutine(DeactivateAfterSeconds(proj, 3f));

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private IEnumerator AttackC()
    {
        if (player == null) yield break;
        isAttacking = true;

        float elapsed = 0f;
        Vector3[] starDirections = GetStarDirections();

        while (elapsed < starDuration)
        {
            foreach (var dir in starDirections)
                FireProjectile(dir);

            elapsed += starFireRate;
            yield return new WaitForSeconds(starFireRate);
        }

        isAttacking = false;
    }

    private IEnumerator AttackD()
    {
        if (player == null) yield break;
        isAttacking = true;

        float elapsed = 0f;
        float currentRotation = 0f;

        while (elapsed < starDuration)
        {
            Vector3[] starDirections = GetRotatedStarDirections(currentRotation);

            foreach (var dir in starDirections)
                FireProjectile(dir);

            currentRotation += starRotationSpeed * starFireRate;
            elapsed += starFireRate;
            yield return new WaitForSeconds(starFireRate);
        }

        isAttacking = false;
    }

    // --- Disparo y desactivación ---
    private void FireProjectile(Vector3 direction)
    {
        GameObject proj = GetProjectileFromPool();
        proj.transform.position = transform.position + direction;
        proj.transform.rotation = Quaternion.LookRotation(direction);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = direction * projectileSpeed;

        StartCoroutine(DeactivateAfterSeconds(proj, 5f));
    }

    private IEnumerator DeactivateAfterSeconds(GameObject proj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (proj.activeSelf)
            ReturnProjectileToPool(proj);
    }

    // --- Utilidades ---
    private Vector3[] GetStarDirections()
    {
        Vector3[] dirs = new Vector3[starProjectiles];
        float angleStep = 360f / starProjectiles;
        for (int i = 0; i < starProjectiles; i++)
        {
            float angle = i * angleStep;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            dirs[i] = rot * transform.forward;
        }
        return dirs;
    }

    private Vector3[] GetRotatedStarDirections(float rotationOffset)
    {
        Vector3[] dirs = new Vector3[starProjectiles];
        float angleStep = 360f / starProjectiles;
        for (int i = 0; i < starProjectiles; i++)
        {
            float angle = i * angleStep + rotationOffset;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            dirs[i] = rot * transform.forward;
        }
        return dirs;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 left = Quaternion.Euler(0, -spreadAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, spreadAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left * 5);
        Gizmos.DrawRay(transform.position, right * 5);

        Gizmos.color = Color.cyan;
        foreach (var dir in GetStarDirections())
            Gizmos.DrawRay(transform.position, dir * 3);
    }
}
