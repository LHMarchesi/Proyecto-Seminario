using UnityEngine;

public class HealthPowerUp : BasePowerUp { // Hereda de BasePowerUP
   
    [SerializeField] MaxHealthStats stats;

    void AddMaxHealth()
    {
        playerContext.PlayerController.AddMaxHealth(stats.newMaxHealth);  //Llama al AddHealth en PlayerController
    }
    protected override void ApplyEffect() 
    {
        AddMaxHealth();
    }

    protected override void Upgrade()
    {
    }
}
