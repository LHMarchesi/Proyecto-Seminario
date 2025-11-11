using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    [Tooltip("Prefab del ítem que puede caer.")]
    public GameObject itemPrefab;

    [Range(0f, 1f), Tooltip("Probabilidad de que este ítem caiga (0 = nunca, 1 = siempre).")]
    public float dropChance = 0.5f;

    [Tooltip("Cantidad mínima que puede caer.")]
    public int minAmount = 1;

    [Tooltip("Cantidad máxima que puede caer.")]
    public int maxAmount = 1;
}
public class EnemyDropManager : MonoBehaviour
{
    [Header("Configuración de Drops")]
    [Tooltip("Lista de ítems posibles a dropear.")]
    public List<DropItem> possibleDrops = new List<DropItem>();

    [Tooltip("Altura a la que spawnean los ítems respecto al enemigo.")]
    public float dropHeightOffset = 1f;

    [Tooltip("Radio aleatorio para esparcir los ítems alrededor del enemigo.")]
    public float scatterRadius = 0.5f;

    /// <summary>
    /// Llamar este método cuando el enemigo muere.
    /// </summary>
    public void DropItems()
    {
        foreach (var drop in possibleDrops)
        {
            if (drop.itemPrefab == null) continue;

            // Tiramos una "moneda" de probabilidad
            if (Random.value <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    // Posición aleatoria alrededor del enemigo
                    Vector3 dropPos = transform.position + Vector3.up * dropHeightOffset;
                    dropPos += new Vector3(
                        Random.Range(-scatterRadius, scatterRadius),
                        0f,
                        Random.Range(-scatterRadius, scatterRadius)
                    );

                    Instantiate(drop.itemPrefab, dropPos, Quaternion.identity);
                }
            }
        }
    }
}
