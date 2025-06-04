using UnityEngine;

public class HealthPowerUp : BasePowerUp {
   
    [SerializeField] float healthAmmount;

    void AddHealth()
    {
        playerContext.PlayerController.AddHealth(healthAmmount);
        Debug.Log("vida aumentada");
    }
    protected override void ApplyEffect()
    {
        AddHealth();
        UIManager.Instance.OnPlayerAddHealth();
    }
}