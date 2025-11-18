using UnityEngine;

public class HealthOrb : OrbBase
{
    [Header("Configuración de Vida")]
    [SerializeField] private int minHeal = 10;
    [SerializeField] private int maxHeal = 25;

    protected override void ApplyEffect(GameObject player)
    {
        int healAmount = Random.Range(minHeal, maxHeal + 1);

        if (player.TryGetComponent(out PlayerContext context))
        {
            context.PlayerController.AddHealth(healAmount);
        }
    }
}
