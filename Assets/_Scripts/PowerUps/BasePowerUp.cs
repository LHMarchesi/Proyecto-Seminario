using System;
using UnityEngine;

public abstract class BasePowerUp : MonoBehaviour, IPickuppeable //Clase Abstracta de la que heredan demas Power Ups
{
    protected PlayerContext playerContext;
    public void PickUp() 
    {
        ApplyEffect();
        Destroy(gameObject);
    }

    protected abstract void ApplyEffect(); // Metodo que utilizan los hijos de esta clase

    private void OnTriggerEnter(Collider other) 
    {
        // Chequear collision con jugador y asignar PlayerContext para acceder a PlayerController y Mjolnir
        playerContext = other.GetComponent<PlayerContext>();

        if (playerContext != null)
            PickUp();
    }
}

public interface IPickuppeable
{
    void PickUp();
}
