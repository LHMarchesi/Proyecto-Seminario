using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class ExperienceOrb : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad con la que el orbe se mueve hacia el jugador.")]
    public float moveSpeed = 5f;

    [Tooltip("Rango en el que el orbe detecta al jugador y empieza a seguirlo.")]
    public float detectionRange = 10f;

    [Tooltip("Altura opcional para mantener el orbe a cierta distancia vertical.")]
    public float hoverHeight = 0.5f;

    [Header("Configuración de Experiencia")]
    [Tooltip("Cantidad mínima de experiencia que puede dar.")]
    public float minExperience = 5f;

    [Tooltip("Cantidad máxima de experiencia que puede dar.")]
    public float maxExperience = 15f;

    [Header("Sonido")]
    [Tooltip("Sonido que se reproducirá al recoger el orbe.")]
    public AudioClip pickupSound;
    private AudioSource audioSource;

    private Transform player;
    private ExperienceManager experienceManager;
    private bool collected = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        experienceManager = FindObjectOfType<ExperienceManager>();

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        if (TryGetComponent(out AudioSource existingSource))
        {
            audioSource = existingSource;
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Si el jugador está dentro del rango, moverse hacia él
        if (distance <= detectionRange)
        {
            Vector3 targetPos = player.position + Vector3.up * hoverHeight;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // Calcular experiencia aleatoria
        float xp = Random.Range(minExperience, maxExperience + 1);

        if (experienceManager != null)
        {
            experienceManager.AddExperience(xp);
        }

        // Reproducir sonido
        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Desactivar visualmente el orbe y destruirlo tras el sonido
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, pickupSound != null ? pickupSound.length : 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f); // Azul translúcido
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
