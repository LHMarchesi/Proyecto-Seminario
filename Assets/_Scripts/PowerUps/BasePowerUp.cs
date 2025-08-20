using System;
using UnityEngine;

public abstract class BasePowerUp : MonoBehaviour, IPickuppeable //Clase Abstracta de la que heredan demas Power Ups
{
    protected PlayerContext playerContext;
    [SerializeField] private bool isTrigger;

    public void PickUp()
    {
        ApplyEffect();
        gameObject.SetActive(false); 
        Destroy(gameObject, 0.3f);
    }

    public void UpgradePowerUp()
    {
        Upgrade();
    }
    protected abstract void ApplyEffect(); // Metodo que utilizan los hijos de esta clase
    protected abstract void Upgrade(); // Metodo que utilizan los hijos de esta clase

    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger)
            return; // Si no es un trigger, no hacer nada

        // Chequear collision con jugador y asignar PlayerContext para acceder a PlayerController y Mjolnir
        playerContext = other.GetComponent<PlayerContext>();

        if (playerContext != null)
            PickUp();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTrigger) return;

        // Chequear collision con jugador y asignar PlayerContext para acceder a PlayerController y Mjolnir
        playerContext = collision.collider.GetComponent<PlayerContext>();

        if (playerContext != null)
            PickUp();
    }

    public void SetPlayerContext(PlayerContext context)
    {
        this.playerContext = context;
    }
}

public interface IPickuppeable
{
    void PickUp();
}
