using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    [Header("Referencia de la puerta")]
    //  public DoorController door; // script que abre la puerta de esta habitación
    public string enemyTag = "Enemy";

    private List<BaseEnemy> enemies = new List<BaseEnemy>();
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            DetectEnemiesInRoom();
        }
      
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
                    enemy.onDeath += () => OnEnemyDie(enemy);
                }
            }
        }

        Debug.Log($"Habitación detectó {enemies.Count} enemigos.");

        CheckIfAllDead();
    }
    void OnEnemyDie(BaseEnemy e)
    {
        e.onDeath -= () => OnEnemyDie(e);
        enemies.Remove(e);
        CheckIfAllDead();
    }

    void CheckIfAllDead()
    {
        if (enemies.Count == 0)
        {
           // door.OpenDoor();
            Debug.Log("Todos los enemigos de la habitación fueron derrotados. Puerta abierta.");
            activated = false;  
        }
    }
}