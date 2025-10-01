using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [Header("Referencia de la puerta")]
    public string enemyTag = "Enemy";
    public TextMeshPro remainingEnemysTxt;
    private List<BaseEnemy> enemies = new List<BaseEnemy>();
    private bool activated = false;
    public List<GameObject> activeDoors;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            DetectEnemiesInRoom();
            DetectDoorsInRoom();
            StartCoroutine(ActivarObjetosConDelay());
        }

    }

    IEnumerator ActivarObjetosConDelay()
    {
        yield return new WaitForSeconds(1.5f);

        foreach (GameObject obj in activeDoors)
            obj.SetActive(true);
    }

    void DetectDoorsInRoom()
    {
        activeDoors.Clear();

        BoxCollider box = GetComponent<BoxCollider>();

        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;

        Collider[] colliders = Physics.OverlapBox(
            worldCenter,
            worldHalfExtents,
            box.transform.rotation);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Door"))
            {
                var door = col.gameObject;
                if (door != null)
                {
                    activeDoors.Add(door);
                    door.SetActive(false);
                }
            }
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
                    enemy.OnDeath += () => OnEnemyDie(enemy);
                }
            }
        }

        remainingEnemysTxt.text = "Enemies remaining: " + enemies.Count.ToString();


        CheckIfAllDead();
    }
    void OnEnemyDie(BaseEnemy e)
    {
        e.OnDeath -= () => OnEnemyDie(e);
        enemies.Remove(e);
        remainingEnemysTxt.text = "Enemies remaining: " + enemies.Count.ToString();
        CheckIfAllDead();
    }

    void CheckIfAllDead()
    {
        if (enemies.Count == 0)
        {
            remainingEnemysTxt.text = "Puerta Abierta";
            foreach (GameObject obj in activeDoors)
                obj.SetActive(false);

            activated = false;
        }
    }
}
