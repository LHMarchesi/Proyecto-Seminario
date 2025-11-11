using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RoomTrigger : MonoBehaviour
{
    [Header("Enemy detection")]
    public string enemyTag = "Enemy";

    [Header("Doors")]
    public List<GameObject> activeDoors;
    public bool isFinalRoom;
    public GameObject finalDoor;

    [Header("Spawning")]
    public bool spawnOnStart = true;

    private bool roomCleared = false;
    [Serializable]
    public class SpawnBatch
    {
        public Transform spawnPoint;          // where to spawn
        public GameObject enemyPrefab;        // what to spawn
        public int count = 1;                 // how many at this point
        public float spreadRadius = 1.0f;     // small radius to avoid overlap
    }
    public List<SpawnBatch> spawnBatches = new List<SpawnBatch>();

    private readonly List<BaseEnemy> enemies = new List<BaseEnemy>();
    private readonly Dictionary<BaseEnemy, Action> deathHandlers = new Dictionary<BaseEnemy, Action>();
    private bool activated = false;
    private bool alreadyCounted;

    private void Start()
    {
        // Close room at start
        foreach (GameObject obj in activeDoors) obj.SetActive(false);
        if (finalDoor != null) finalDoor.SetActive(false);

        // Spawn configured enemies
        if (spawnOnStart && spawnBatches != null)
            SpawnConfiguredEnemies();
    }

    private void SpawnConfiguredEnemies()
    {
        roomCleared = false;
        alreadyCounted = false;

        foreach (var batch in spawnBatches)
        {
            if (batch == null || batch.spawnPoint == null || batch.enemyPrefab == null || batch.count <= 0)
                continue;

            int adjustedCount = Mathf.RoundToInt(batch.count * DifficultyManager.EnemyMultiplier);

            for (int i = 0; i < adjustedCount; i++)
            {
                Vector3 offset2D = (batch.spreadRadius > 0f)
                    ? UnityEngine.Random.insideUnitCircle * batch.spreadRadius
                    : Vector2.zero;

                Vector3 spawnPos = batch.spawnPoint.position + new Vector3(offset2D.x, 0f, offset2D.y);
                Quaternion spawnRot = batch.spawnPoint.rotation;

                GameObject go = Instantiate(batch.enemyPrefab, spawnPos, spawnRot);

                if (!string.IsNullOrEmpty(enemyTag)) go.tag = enemyTag;

                var enemy = go.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                    SafeSubscribe(enemy);

                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(ActivateDoorsWithDelay());
            UIManager.Instance.UpdateEnemiesRemaining(true, enemies.Count);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            DetectEnemiesInRoom();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            UIManager.Instance.UpdateEnemiesRemaining(false, 0);
    }

    IEnumerator ActivateDoorsWithDelay()
    {
        yield return new WaitForSeconds(1.5f);
        foreach (GameObject obj in activeDoors) obj.SetActive(true);
    }

    void DetectEnemiesInRoom()
    {
        enemies.Clear();

        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;

        Collider[] colliders = Physics.OverlapBox(worldCenter, worldHalfExtents, box.transform.rotation);

        foreach (var col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                var enemy = col.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                    // Subscribe once per enemy instance
                    if (!deathHandlers.ContainsKey(enemy))
                        SafeSubscribe(enemy);
                }
            }
        }

        UIManager.Instance.UpdateEnemiesRemaining(true, enemies.Count);
        CheckIfAllDead();
    }

    void SafeSubscribe(BaseEnemy enemy)
    {
        // Cache handler so we can unsubscribe correctly later
        Action handler = null;
        handler = () => OnEnemyDie(enemy);
        deathHandlers[enemy] = handler;
        enemy.OnDeath += handler;
    }

    void OnEnemyDie(BaseEnemy e)
    {
        if (deathHandlers.TryGetValue(e, out var handler))
        {
            e.OnDeath -= handler;
            deathHandlers.Remove(e);
        }

        enemies.Remove(e);
        CheckIfAllDead();
        UIManager.Instance.UpdateEnemiesRemaining(true, enemies.Count);
    }

    void CheckIfAllDead()
    {
        if (enemies.Count == 0 && !alreadyCounted && activated)
        {
            alreadyCounted = true; // Evita repetir el conteo

            foreach (GameObject obj in activeDoors)
                obj.SetActive(false);

            activated = false;

            // Subir dificultad
       //     DifficultyManager.Instance.IncreaseDifficulty();

            if (isFinalRoom && finalDoor != null)
                finalDoor.SetActive(true);
        }
    }

}
