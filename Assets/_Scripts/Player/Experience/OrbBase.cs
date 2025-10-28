using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public abstract class OrbBase : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    [Tooltip("Velocidad con la que el orbe se mueve hacia el jugador.")]
    [SerializeField] protected float moveSpeed = 10f;

    [Tooltip("Rango en el que el orbe detecta al jugador y empieza a seguirlo.")]
    [SerializeField] protected float detectionRange = 10f;

    [Tooltip("Altura opcional para mantener el orbe a cierta distancia vertical.")]
    [SerializeField] protected float hoverHeight = 0.5f;

    [Header("Sonido")]
    [Tooltip("Sonido que se reproducir� al recoger el orbe.")]
    [SerializeField] protected AudioClip pickupSound;
    protected AudioSource audioSource;

    protected Transform player;
    protected bool collected = false;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

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

    protected virtual void Update()
    {
        if (player == null || collected) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Vector3 targetPos = player.position + Vector3.up * hoverHeight;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        ApplyEffect(other.gameObject);

        if (pickupSound != null)
            audioSource.PlayOneShot(pickupSound);

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, pickupSound != null ? pickupSound.length : 0.1f);
    }

    protected abstract void ApplyEffect(GameObject player);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
