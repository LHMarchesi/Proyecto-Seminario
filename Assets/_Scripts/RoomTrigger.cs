using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [Header("Referencia de la puerta")]
    public string enemyTag = "Enemy";

    private List<BaseEnemy> enemies = new List<BaseEnemy>();
    private bool activated = false;
    public List<GameObject> objetosActivables;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            DetectEnemiesInRoom();
            StartCoroutine(ActivarObjetosConDelay());
        }

    }

    IEnumerator ActivarObjetosConDelay()
    {
        yield return new WaitForSeconds(5f);

        foreach (GameObject obj in objetosActivables)
            obj.SetActive(true);
    }



    void DetectEnemiesInRoom()
    {
        enemies.Clear();

        BoxCollider box = GetComponent<BoxCollider>();

        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;

        Collider[] colliders = Physics.OverlapBox(
            worldCenter,
            worldHalfExtents,
            box.transform.rotation);

        foreach (var col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                var enemy = col.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                    enemy.OnDeath += () => OnEnemyDie(enemy);
                }
            }
        }

        Debug.Log($"Habitaci�n detect� {enemies.Count} enemigos.");



        CheckIfAllDead();
    }
    void OnEnemyDie(BaseEnemy e)
    {
        e.OnDeath -= () => OnEnemyDie(e);
        enemies.Remove(e);
        CheckIfAllDead();
    }

    void CheckIfAllDead()
    {
        if (enemies.Count == 0)
        {
            // door.OpenDoor();
            Debug.Log("Todos los enemigos de la habitaci�n fueron derrotados. Puerta abierta.");
            foreach (GameObject obj in objetosActivables)
                obj.SetActive(false);
            activated = false;
        }
    }
}
