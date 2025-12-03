using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RoomTrigger : MonoBehaviour
{
    [Header("Doors")]
    public List<GameObject> activeDoors;

    public List<ObstacleDestructible> activePillars = new List<ObstacleDestructible>();
    private Dictionary<ObstacleDestructible, Action> deathHandlers = new Dictionary<ObstacleDestructible, Action>();
    private bool activated = false;
    private bool alreadyCounted;




    private void Start()
    {

        foreach (GameObject obj in activeDoors) obj.SetActive(false); // Open doors at start

    }

    public void ForceOpenDoors()
    {
        foreach (GameObject obj in activeDoors)
            obj.SetActive(false);


        activated = false;
        alreadyCounted = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!activated)
            {
                activated = true;
                StartCoroutine(ActivateDoorsWithDelay());
                UIManager.Instance.UpdateEnemiesRemaining(true, activePillars.Count);
            }
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
        {
            UIManager.Instance.UpdateEnemiesRemaining(false, 0);
        }
    }

    IEnumerator ActivateDoorsWithDelay()
    {
        yield return new WaitForSeconds(1f);
        foreach (GameObject obj in activeDoors) obj.SetActive(true);
    }

    void DetectEnemiesInRoom()
    {
        if (!activated) return;

        activePillars.Clear();

        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;

        Collider[] colliders = Physics.OverlapBox(worldCenter, worldHalfExtents, box.transform.rotation);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Pillar"))
            {
                var píllar = col.GetComponent<ObstacleDestructible>();
                if (píllar != null)
                {
                    activePillars.Add(píllar);
                    // Subscribe once per enemy instance
                    if (!deathHandlers.ContainsKey(píllar))
                        SafeSubscribe(píllar);
                }
            }
        }

        UIManager.Instance.UpdateEnemiesRemaining(true, activePillars.Count);
    }

    void SafeSubscribe(ObstacleDestructible pillar)
    {
        // Cache handler so we can unsubscribe correctly later
        Action handler = null;
        handler = () => OnPillarDestroy(pillar);
        deathHandlers[pillar] = handler;
        pillar.OnDestroy += handler;
    }

    void OnPillarDestroy(ObstacleDestructible pillar)
    {
        if (deathHandlers.TryGetValue(pillar, out var handler))
        {
            pillar.OnDestroy -= handler;
            deathHandlers.Remove(pillar);
        }

        activePillars.Remove(pillar);
        CheckIfAllDead();
        UIManager.Instance.UpdateEnemiesRemaining(true, activePillars.Count);
    }

    void CheckIfAllDead()
    {
        if (activePillars.Count == 0 && !alreadyCounted && activated)
        {
            alreadyCounted = true; // Evita repetir el conteo

            foreach (GameObject obj in activeDoors)
                obj.SetActive(false);

            activated = false;
            Destroy(gameObject);
            // Subir dificultad
            //     DifficultyManager.Instance.IncreaseDifficulty();
        }
    }

}
