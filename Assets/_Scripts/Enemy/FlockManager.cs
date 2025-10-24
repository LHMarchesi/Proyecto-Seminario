using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager Instance;

    [Header("Spatial Grid Settings")]
    [Tooltip("Tamaño de cada celda usada para optimizar la búsqueda de vecinos.")]
    public float cellSize = 10f;

    private readonly Dictionary<Vector2Int, List<EnemyFlockBehaviour>> grid = new Dictionary<Vector2Int, List<EnemyFlockBehaviour>>();
    private readonly List<EnemyFlockBehaviour> agents = new List<EnemyFlockBehaviour>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterAgent(EnemyFlockBehaviour agent)
    {
        if (!agents.Contains(agent))
            agents.Add(agent);
    }

    public void UnregisterAgent(EnemyFlockBehaviour agent)
    {
        agents.Remove(agent);
    }

    private void LateUpdate()
    {
        grid.Clear();

        foreach (var agent in agents)
        {
            if (agent == null) continue;
            Vector2Int cell = GetCell(agent.transform.position);
            if (!grid.ContainsKey(cell))
                grid[cell] = new List<EnemyFlockBehaviour>();
            grid[cell].Add(agent);
        }
    }

    private Vector2Int GetCell(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    /// <summary>
    /// Devuelve una lista de agentes cercanos a este, usando la grilla espacial.
    /// </summary>
    public List<EnemyFlockBehaviour> GetNearbyAgents(EnemyFlockBehaviour agent)
    {
        List<EnemyFlockBehaviour> nearby = new List<EnemyFlockBehaviour>();
        Vector2Int centerCell = GetCell(agent.transform.position);

        // Buscar en la celda actual y las 8 adyacentes
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int neighborCell = new Vector2Int(centerCell.x + x, centerCell.y + z);
                if (grid.ContainsKey(neighborCell))
                    nearby.AddRange(grid[neighborCell]);
            }
        }

        return nearby;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Verde translúcido

        foreach (var cell in grid.Keys)
        {
            Vector3 cellWorldPos = new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
            Gizmos.DrawWireCube(cellWorldPos + new Vector3(cellSize / 2f, 0f, cellSize / 2f), new Vector3(cellSize, 0.1f, cellSize));
        }
    }
#endif
}
