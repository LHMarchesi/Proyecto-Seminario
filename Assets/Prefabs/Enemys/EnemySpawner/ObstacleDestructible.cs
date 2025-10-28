using System;
using UnityEngine;

public class ObstacleDestructible : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth = 50f;
    private float damageCooldown = 0.2f; // medio segundo de invulnerabilidad
    private float lastDamageTime = -Mathf.Infinity;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Transform effectSpawnPos;

    public void TakeDamage(float damage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;
        OnDamage(damage);
    }

    private void OnDamage(float damage)
    {
        //OnDamageEffect

        currentHealth -= damage;
        if (currentHealth <= 0)
            DestroyObject();

        
    }

    private void DestroyObject()
    {
        DifficultyUp();
        GetComponent<EnemyDropManager>()?.DropItems();
        Instantiate(destroyEffect, effectSpawnPos);
        Destroy(gameObject);
    }

    private void DifficultyUp()
    {
        Debug.Log("Dificultad Aumentada");
    }
}
