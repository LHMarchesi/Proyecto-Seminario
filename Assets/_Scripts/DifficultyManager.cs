using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Multipliers por nivel de dificultad")]
    [SerializeField] private float healthMultiplier = 1.1f;
    [SerializeField] private float speedMultiplier = 1.05f;
    [SerializeField] private float damageMultiplier = 1.1f;
    [SerializeField] private float expMultiplier = 1.2f;

    [Header("Multiplicador de cantidad de enemigos")]
    [SerializeField] private float enemyMultiplierIncrement = 0.2f; // +20% enemigos por nivel
    public static float EnemyMultiplier { get; private set; } = 1f;

    [Header("Progresión de dificultad")]
    [SerializeField] private int currentDifficultyLevel = 1;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    public static DifficultyManager Instance { get; private set; }
    public bool IsNewRun { get; private set; } = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        //StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.5f); // Espera a que los enemigos estén en escena
     //   ResetDifficulty();
    }

    public void ResetDifficulty()
    {
        currentDifficultyLevel = 1;
        EnemyMultiplier = 1f;
        IsNewRun = true;

        foreach (var enemy in activeEnemies)
        {
          //  enemy.ResetStatsToBase();
        }

        Debug.Log("Dificultad y estadísticas de enemigos reiniciadas al empezar una nueva partida.");
    }

    public void RegisterEnemy(BaseEnemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void ScaleEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            ApplyDifficultyScaling(enemy);
        }
    }

    public void IncreaseDifficulty()
    {
        currentDifficultyLevel++;
        IsNewRun = false;

        // Escalar estadísticas
        ScaleEnemies();

        // Aumentar la cantidad de enemigos futuros
        EnemyMultiplier += enemyMultiplierIncrement;

        Debug.Log($"Dificultad aumentada a nivel {currentDifficultyLevel}. EnemyMultiplier = {EnemyMultiplier:F2}");
    }

    private void ApplyDifficultyScaling(BaseEnemy enemy)
    {
        float healthBonus = (enemy.Stats.maxHealth * (healthMultiplier - 1)) * currentDifficultyLevel;
        float speedBonus = (enemy.Stats.moveSpeed * (speedMultiplier - 1)) * currentDifficultyLevel;
        float damageBonus = (enemy.Stats.attackDamage * (damageMultiplier - 1)) * currentDifficultyLevel;
        float expBonus = (enemy.Stats.expDrop * (expMultiplier - 1)) * currentDifficultyLevel;

        enemy.AddMaxHealth(healthBonus);
        enemy.AddMaxSpeed(speedBonus);
        enemy.AddMaxAttackDamage(damageBonus);
        enemy.AddMaxExpDrop(expBonus);
    }
}



