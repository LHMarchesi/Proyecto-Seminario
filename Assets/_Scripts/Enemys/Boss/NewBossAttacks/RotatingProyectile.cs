using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingProyectile : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 10f;
    public float lifeTime = 6f;

    [Header("Burst Settings")]
    public GameObject subProjectilePrefab;
    public int burstProjectileCount = 8;
    public float burstInterval = 0.25f;
    public float burstDelay = 0f;
    public float burstProjectileSpeed = 12f;
    public float subProjectileLifetime = 3f;

    [Header("Rotación")]
    public float rotationSpeed = 180f;  // grados por segundo
    public bool clockwise = true;

    [Header("Gizmos")]
    public float gizmoLineLength = 3f;
    public Color gizmoColor = Color.magenta;

    // estado interno
    private float timer = 0f;
    private float burstTimer = 0f;
    private Vector3 movementDirection; // <- dirección fija de movimiento (world-space)

    private void Start()
    {
        // Guardamos la dirección inicial en world-space. 
        // Usamos transform.forward en el momento del spawn (asumimos que está correctamente orientado al instante).
        movementDirection = transform.forward.normalized;

        burstTimer = -burstDelay; // respetar burstDelay (si 0 -> primer burst inmediato)
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1) Mover en línea recta usando movementDirection (no dependemos de transform.forward)
        transform.position += movementDirection * speed * dt;

        // 2) Rotación VISUAL del transform para que los bursts roten con él
        float dirSign = clockwise ? 1f : -1f;
        transform.Rotate(Vector3.up, rotationSpeed * dirSign * dt, Space.Self);

        // 3) Timers
        timer += dt;
        burstTimer += dt;

        // 4) Generar bursts periódicos mientras esté vivo
        if (timer < lifeTime && burstTimer >= burstInterval)
        {
            GenerateBurst();
            burstTimer = 0f;
        }

        // 5) Auto-destruir el proyectil principal al terminar su lifetime
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void GenerateBurst()
    {
        if (subProjectilePrefab == null) return;

        float step = 360f / Mathf.Max(1, burstProjectileCount);

        for (int i = 0; i < burstProjectileCount; i++)
        {
            // rotación relativa según la rotación actual del transform (esto hace que la estrella "gire")
            Quaternion rot = Quaternion.Euler(0, i * step, 0);
            Vector3 dir = transform.rotation * rot * Vector3.forward;

            GameObject sub = Instantiate(subProjectilePrefab, transform.position, Quaternion.LookRotation(dir));

            // Si el sub-proyectil usa Rigidbody, le damos velocidad
            Rigidbody rb = sub.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = dir * burstProjectileSpeed;
            }

            // Controlamos el lifetime desde aquí (no hace falta tocar el prefab)
            Destroy(sub, subProjectileLifetime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;

        float step = 360f / Mathf.Max(1, burstProjectileCount);

        for (int i = 0; i < burstProjectileCount; i++)
        {
            Quaternion rot = Quaternion.Euler(0, i * step, 0);
            Vector3 dir = transform.rotation * rot * Vector3.forward;
            Gizmos.DrawLine(transform.position, transform.position + dir * gizmoLineLength);
            Gizmos.DrawSphere(transform.position + dir * gizmoLineLength, 0.06f);
        }

        // Dibuja también la línea de movimiento fija para verificar que va recto
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + movementDirection * gizmoLineLength);
    }
}
