using System;
using UnityEngine;

public abstract class BasePowerUp : MonoBehaviour, IPickuppeable //Clase Abstracta de la que heredan demas Power Ups
{
    protected PlayerContext playerContext;

    [Header("Pickup legacy")]
    [SerializeField] private bool isTrigger;

    private bool acquiredForRun;

    public bool IsAcquiredForRun => acquiredForRun;

    // Usado por RunInventory. El objeto queda vivo durante toda la run.
    public void AcquireForRun(PlayerContext context)
    {
        if (acquiredForRun || context == null)
            return;

        playerContext = context;
        acquiredForRun = true;

        DisablePickupPresentation();
        ApplyEffect();
    }

    // Mantiene compatibilidad con pickups viejos del mundo.
    // En vez de destruir el componente, lo conserva como lógica runtime.
    public void PickUp()
    {
        if (playerContext == null)
            return;

        transform.SetParent(playerContext.transform);
        transform.localPosition = Vector3.zero;
        AcquireForRun(playerContext);
    }

    public void UpgradePowerUp()
    {
        if (!acquiredForRun)
            return;

        Upgrade();
    }

    protected abstract void ApplyEffect();
    protected abstract void Upgrade();

    protected T CreateRuntimeStatsCopy<T>(T source) where T : ScriptableObject
    {
        return source != null ? Instantiate(source) : null;
    }

    private void DisablePickupPresentation()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
            col.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
            renderer.enabled = false;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.detectCollisions = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger || acquiredForRun)
            return;

        PlayerContext context = other.GetComponentInParent<PlayerContext>();
        if (context == null)
            return;

        playerContext = context;
        PickUp();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTrigger || acquiredForRun)
            return;

        PlayerContext context = collision.collider.GetComponentInParent<PlayerContext>();
        if (context == null)
            return;

        playerContext = context;
        PickUp();
    }

    public void SetPlayerContext(PlayerContext context)
    {
        playerContext = context;
    }
}

public interface IPickuppeable
{
    void PickUp();
}
