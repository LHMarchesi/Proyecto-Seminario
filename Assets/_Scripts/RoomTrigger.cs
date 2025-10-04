using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [Header("Referencia de la puerta")]
    public string enemyTag = "Enemy";
    public TextMeshPro remainingEnemysTxt;
    private List<BaseEnemy> enemies = new List<BaseEnemy>();
    private bool activated = false;
    public List<GameObject> activeDoors;


    private void Start()
    {
        foreach (GameObject obj in activeDoors)
            obj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(ActivateDoorsWithDelay());
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DetectEnemiesInRoom();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.UpdateEnemiesRemaining(false, 0);
        }
    }

    IEnumerator ActivateDoorsWithDelay()
    {
        yield return new WaitForSeconds(1.5f);

        // SoundManager.Instance.PlaySFX(SoundManager.Sounds.DoorClosed);

        foreach (GameObject obj in activeDoors)
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

        UIManager.Instance.UpdateEnemiesRemaining(true, enemies.Count);

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
            foreach (GameObject obj in activeDoors)
                obj.SetActive(false);

            activated = false;

            // SoundManager.Instance.PlaySFX(SoundManager.Sounds.DoorOpen);
        }
    }
}
