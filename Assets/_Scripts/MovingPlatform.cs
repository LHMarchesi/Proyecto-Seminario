using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    public Vector3 puntoA;         // Posición inicial
    public Vector3 puntoB;         // Posición final
    public float velocidad = 2f;

    private Vector3 destinoActual;

    void Start()
    {
        // El primer destino será el punto A
        destinoActual = puntoA;
    }

    void Update()
    {
        // Mover la plataforma hacia el destino
        transform.position = Vector3.MoveTowards(transform.position, destinoActual, velocidad * Time.deltaTime);

        // Cuando llegue al destino, cambiar al otro
        if (Vector3.Distance(transform.position, destinoActual) < 0.05f)
        {
            if (destinoActual == puntoA)
                destinoActual = puntoB;
            else
                destinoActual = puntoA;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
