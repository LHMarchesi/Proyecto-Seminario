using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZoneCheck : MonoBehaviour
{
    public List<GameObject> objetosActivables;
    public float tiempoParaActivar = 2f;
    public float tiempoParaDesactivar = 2f;

    private HashSet<GameObject> enemigosDentro = new HashSet<GameObject>();
    private bool activados = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activados)
        {
            StartCoroutine(ActivarObjetosConDelay());
        }

        if (other.CompareTag("Enemy"))
        {
            enemigosDentro.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemigosDentro.Remove(other.gameObject);

            if (enemigosDentro.Count == 0 && activados)
            {
                StartCoroutine(DesactivarObjetosConDelay());
            }
        }
    }

    IEnumerator ActivarObjetosConDelay()
    {
        yield return new WaitForSeconds(tiempoParaActivar);

        foreach (GameObject obj in objetosActivables)
            obj.SetActive(true);

        activados = true;
    }

    IEnumerator DesactivarObjetosConDelay()
    {
        yield return new WaitForSeconds(tiempoParaDesactivar);

        foreach (GameObject obj in objetosActivables)
            obj.SetActive(false);

        activados = false;
    }
}
