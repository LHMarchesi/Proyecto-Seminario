using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinLightCode : MonoBehaviour
{
    [SerializeField] private float velocidadRotacion = 45f; // Grados por segundo

    void Update()
    {
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f);
    }
}
