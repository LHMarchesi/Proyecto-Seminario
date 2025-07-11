using UnityEngine;

public class HealthPowerUp : BasePowerUp { // Hereda de BasePowerUP
   
    [SerializeField] MaxHealthStats stats;

    void AddHealth()
    {
        playerContext.PlayerController.AddHealth(stats.newMaxHealth);  //Llama al AddHealth en PlayerController
    }
    protected override void ApplyEffect() 
    {
        AddHealth();
    }

    protected override void Upgrade()
    {
    }
}
