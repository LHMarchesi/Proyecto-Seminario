using System;
using UnityEngine;

public class ObstacleDestructible : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth = 50f;
    private float damageCooldown = 0.2f;
    private float lastDamageTime = -Mathf.Infinity;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Transform effectSpawnPos;

    private bool canTakeDamage = true; // ← NUEVO

    public void TakeDamage(float damage)
    {
        if (!canTakeDamage) return; // ← bloquea daño si está desactivado
        if (Time.time - lastDamageTime < damageCooldown) return;

        OnDamage(damage);
        lastDamageTime = Time.time;
    }

    private void OnDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            DestroyObject();
    }

    private void DestroyObject()
    {
        GetComponent<EnemyDropManager>()?.DropItems();
        Instantiate(destroyEffect, effectSpawnPos.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // 🔥 NUEVO: control del daño
    public void SetCanTakeDamage(bool state)
    {
        canTakeDamage = state;
    }
}
