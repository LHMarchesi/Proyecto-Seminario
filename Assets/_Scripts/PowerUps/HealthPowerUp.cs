using UnityEngine;

public class HealthPowerUp : BasePowerUp { // Hereda de BasePowerUP
   
    [SerializeField] float healthAmmount;

    void AddHealth()
    {
        playerContext.PlayerController.AddHealth(healthAmmount);  //Llama al AddHealth en PlayerController
        Debug.Log("vida aumentada");
    }
    protected override void ApplyEffect() 
    {
        AddHealth();
        UIManager.Instance.OnPlayerAddHealth(); //Flash verde en UI
    }
}