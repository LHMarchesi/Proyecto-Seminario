using System;
using UnityEngine;

public class ObstacleDestructible : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth = 50f;
    private float damageCooldown = 0.2f; // medio segundo de invulnerabilidad
    private float lastDamageTime = -Mathf.Infinity;

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
        Destroy(gameObject);
        //Destroy Effect
    }
}
