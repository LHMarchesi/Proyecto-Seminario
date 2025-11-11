using UnityEngine;

public class HealthOrb : OrbBase
{
    [Header("Configuración de Vida")]
    [SerializeField] private float minHeal = 10f;
    [SerializeField] private float maxHeal = 25f;

    protected override void ApplyEffect(GameObject player)
    {
        float healAmount = Random.Range(minHeal, maxHeal + 1);

        if (player.TryGetComponent(out PlayerContext context))
        {
            context.PlayerController.AddHealth(healAmount);
        }
    }
}
