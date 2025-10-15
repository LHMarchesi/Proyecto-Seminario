using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomThunderScript : MonoBehaviour
{
    [Header("Prefab y Pool")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private int poolSize = 3;

    [Header("Rango de Spawn (en unidades alrededor del spawner)")]
    [SerializeField] private Vector3 range = new Vector3(10, 0, 10);

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float objectLifetime = 5f;

    private List<GameObject> pool;
    private Coroutine spawnRoutine;

    private void Start()
    {
        InitializePool();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void InitializePool()
    {
        pool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefabToSpawn);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    private IEnumerator SpawnLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            SpawnObject();
            yield return wait;
        }
    }

    private void SpawnObject()
    {
        GameObject obj = GetPooledObject();
        if (obj == null) return;

        // El centro es la posición del GameObject con este script
        Vector3 center = transform.position;

        // Calculamos una posición aleatoria dentro del rango
        Vector3 randomPos = new Vector3(
            center.x + Random.Range(-range.x / 2f, range.x / 2f),
            center.y + Random.Range(-range.y / 2f, range.y / 2f),
            center.z + Random.Range(-range.z / 2f, range.z / 2f)
        );

        obj.transform.position = randomPos;
        obj.SetActive(true);

        StartCoroutine(DeactivateAfterSeconds(obj, objectLifetime));
    }

    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // Si el pool está lleno, opcionalmente expandirlo:
        GameObject newObj = Instantiate(prefabToSpawn);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    private IEnumerator DeactivateAfterSeconds(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, range);
    }
}
