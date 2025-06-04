using System;
using UnityEngine;

public abstract class BasePowerUp : MonoBehaviour, IPickuppeable
{
    protected PlayerContext playerContext;
    public void PickUp()
    {
        ApplyEffect();
        Destroy(gameObject);
    }

    protected abstract void ApplyEffect();

    private void OnTriggerEnter(Collider other)
    {
        playerContext = other.GetComponent<PlayerContext>();

        if (playerContext != null)
            PickUp();
    }
}

public interface IPickuppeable
{
    void PickUp();
}
