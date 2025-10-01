using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Configuración de oleadas")]
    public int maxWaves = 5;                 

    [Header("Prefabs y Spawnpoints")]
    public List<GameObject> enemyPrefabs;    
    public Transform[] spawnPoints;          

    private int currentWave = 0;
    public int enemiesToSpawn = 5;          
    private bool spawning = false;
    private bool wavesStarted = false;       

    private List<GameObject> aliveEnemies = new List<GameObject>(); 

    public List<GameObject> objetosActivables;


    void Update()
    {
        if (!wavesStarted) return; 

        
        if (!spawning && aliveEnemies.Count == 0)
        {
            if (currentWave < maxWaves)
            {
                StartCoroutine(SpawnWave());
            }
            else
            {
                Debug.Log("Todas las oleadas completadas!");
                foreach (GameObject obj in objetosActivables)
                    obj.SetActive(false);
            }
        }
    }

    IEnumerator SpawnWave()
    {
        spawning = true;
        currentWave++;

        Debug.Log("🌊 Oleada " + currentWave + " iniciada!");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f); // pequeño delay entre spawns (opcional)
        }


        enemiesToSpawn += Random.Range(4, 6);

        spawning = false;
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay enemigos o spawn points configurados!");
            return;
        }

        // Elegir enemigo y spawnpoint aleatorios
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Registrar al enemigo en la lista
        aliveEnemies.Add(newEnemy);

        // Enlazar callback para cuando muera
        BaseEnemy enemyComponent = newEnemy.GetComponent<BaseEnemy>();
        if (enemyComponent != null)
        {
            enemyComponent.OnDeath += () => { aliveEnemies.Remove(newEnemy); };
        }
        else
        {
            Debug.LogWarning("El prefab del enemigo no tiene componente Enemy con evento onDeath.");
        }
    }

    //Trigger para iniciar las oleadas
    private void OnTriggerEnter(Collider other)
    {
        if (!wavesStarted && other.CompareTag("Player"))
        {
            wavesStarted = true;
            Debug.Log("▶El jugador activó el WaveSpawner");

            StartCoroutine(ActivarObjetosConDelay());

            // Desactivar el trigger para que no vuelva a activarse
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.enabled = false;
            }
        }
    }

    IEnumerator ActivarObjetosConDelay()
    {
        yield return new WaitForSeconds(5f);

        foreach (GameObject obj in objetosActivables)
            obj.SetActive(true);
    }
}
