using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMjolnirRetractBehavior
{
    void OnRetract(Mjolnir mjolnir);
}

public class Mjolnir : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerContext playerContext;
    private Quaternion startRotation;

    [Header("References")]
    [SerializeField] private Transform hand;

    [Header("Settings")]
    [SerializeField] private float minThrowPower;
    [SerializeField] private float maxThrowPower;
    [SerializeField] private float torqueForce;
    [SerializeField] private float maxRetractPower;
    [SerializeField] private float damage;
    //public float parryCooldown;
    // public float nextParryCD;

    public Action<Collider> OnHitEnemy;
    public Action OnMjolnirThrow;
    public Action OnMjolnirRetract;
    public Action OnChrgingThrow;

    private bool isHeld;
    private bool isRetracting;
    public bool teleport;
    public bool parry;

    private float throwChargeTime = 0f;
    private float maxChargeTime = 3;
    private bool isChargingThrow = false;
    private bool wasThrowing = false;
    private Vector3 originalSize;

    // --- VFX on hit (add-only) ---
    [Header("VFX")]
    [SerializeField] private GameObject hitVFXPrefab;   // Prefab with ParticleSystem or VFX Graph
    [SerializeField] private float hitVFXLifetime = 2f; // Safety destroy time
    [SerializeField] private bool parentVFXToHit = false; // Stick effect to the hit object

    private readonly List<IMjolnirRetractBehavior> retractBehaviors = new();
    public void RegisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (!retractBehaviors.Contains(behavior))
            retractBehaviors.Add(behavior);
    }
    public void UnregisterRetractBehavior(IMjolnirRetractBehavior behavior)
    {
        if (retractBehaviors.Contains(behavior))
            retractBehaviors.Remove(behavior);
    }


    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerContext = GetComponentInParent<PlayerContext>();

        startRotation = transform.localRotation;
        originalSize = transform.localScale;
        Catch();
    }


    void Update()
    {
        // HADNLE THROW //
        //
        bool isCurrentlyThrowing = playerContext.HandleInputs.IsThrowing();
        if (isHeld && isCurrentlyThrowing)
        {
            if (!isChargingThrow)
                isChargingThrow = true;  // Start charging the throw

            throwChargeTime += Time.deltaTime * 2;  // Increment time charge
            throwChargeTime = Mathf.Clamp(throwChargeTime, 0f, maxChargeTime);


        }
        else if (isChargingThrow && wasThrowing && !isCurrentlyThrowing) // Throw at button release
        {
            Throw();
            throwChargeTime = 0f;  // Reset throw
            isChargingThrow = false;
        }
        wasThrowing = isCurrentlyThrowing; // Save state 

        // HADNLE CATCH //
        //
        if (!isHeld && playerContext.HandleInputs.IsCatching())
            isRetracting = true;
    }

    private void FixedUpdate()
    {
        if (isRetracting)
        {
            Retract();
        }
    }

    public void Throw()
    {
        OnMjolnirThrow?.Invoke();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        isHeld = false;

        Vector3 cameraForward = Camera.main.transform.forward; // Calculate direction towards camera
        float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);  // Calculate the force acordding to the loading time

        transform.parent = null;

        // Apply force and torque
        rb.AddForce(SearchForCloseEnemies() * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    public Vector3 SearchForCloseEnemies()
    {
        float searchRadius = 40f;
        float maxAngle = 30f; // Ángulo del cono de visión, en grados
        LayerMask enemyMask = LayerMask.GetMask("HammerTarget");

        // Obtener enemigos en un radio general
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyMask);

        Transform bestTarget = null;
        float bestScore = -1f;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraPosition = Camera.main.transform.position;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitWAim, 60f, enemyMask))
        {
            return (hitWAim.transform.position - transform.position).normalized;
        }

        foreach (var hit in hits)
        {
            Vector3 toEnemy = (hit.transform.position - cameraPosition).normalized;
            float angle = Vector3.Angle(cameraForward, toEnemy);

            // Ignorar enemigos fuera del cono frontal
            if (angle > maxAngle) continue;

            // Calcular una "puntuación" de prioridad: más cerca y más centrado
            float distance = Vector3.Distance(cameraPosition, hit.transform.position);
            float score = Mathf.Lerp(1f, 0f, angle / maxAngle) / distance;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget != null)
        {
            // Direccion hacia el objetivo elegido
            Vector3 directionToTarget = (bestTarget.position - transform.position).normalized;
            return directionToTarget;
        }

        // Si no hay enemigos válidos, lanzar hacia adelante
        return cameraForward;
    }


    public void ThrowWithPower(float powerMultiplier = 1f)
    {
        OnMjolnirThrow?.Invoke();

        isRetracting = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;


        Vector3 cameraForward = Camera.main.transform.forward;
        float finalThrowPower = maxThrowPower * powerMultiplier;
        Debug.Log(finalThrowPower);

        rb.AddForce(cameraForward.normalized * finalThrowPower, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.right * torqueForce, ForceMode.VelocityChange);
    }

    public void ShowMessage(string message) { Debug.Log(message); }

    private void Retract()
    {
        if (isHeld) return;     // Avoid running if already held

        OnMjolnirRetract?.Invoke();

        foreach (var behavior in retractBehaviors)
        {
            behavior.OnRetract(this);
        }

        Vector3 directionToHand = hand.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToHand);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        rb.isKinematic = true; // Detenemos la f�sica
        transform.position = Vector3.MoveTowards(transform.position, hand.position, maxRetractPower * Time.deltaTime);


        if (Vector3.Distance(hand.position, transform.position) < .5f)
        {
            Catch();
        }
        else if (Vector3.Distance(hand.position, transform.position) < .15f)
        {
            rb.isKinematic = false;
            Vector3 cameraForward = Camera.main.transform.forward;
            float finalThrowPower = Mathf.Lerp(minThrowPower, maxThrowPower, throwChargeTime);
            isRetracting = false;
            bool isCurrentlyThrowing = playerContext.HandleInputs.IsThrowing();
            rb.AddForce(cameraForward.normalized * finalThrowPower * 5f, ForceMode.VelocityChange);
        }
        this.transform.localScale = originalSize;

        transform.localScale = Vector3.Lerp(transform.localScale, originalSize, Time.deltaTime * 10f);
    }

    public void Catch()
    {

        isHeld = true;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        isRetracting = false;

        transform.parent = hand; // Assing to the hand
        transform.localPosition = Vector3.zero;
        transform.localRotation = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isHeld) return;

        IDamageable damageable = collision.collider.GetComponent<IDamageable>();

        if (damageable == playerContext.PlayerController.GetComponent<IDamageable>()) //Dont damage player
            return;

        if (damageable != null)
        {
            damageable?.TakeDamage(damage);
            OnHitEnemy?.Invoke(collision.collider);

            var enemy = collision.collider.GetComponent<BaseEnemy>();
            if (enemy != null && enemy.IsDead())
            {
                playerContext.PlayerStateMachine.ChangeState(playerContext.PlayerStateMachine.catchingState);
                isRetracting = true;
            }

            // --- spawn VFX on hit (add-only) ---
            if (hitVFXPrefab != null && collision.contactCount > 0)
            {
                var contact = collision.GetContact(0);
                Vector3 spawnPos = contact.point;
                Quaternion spawnRot = Quaternion.LookRotation(contact.normal);
                GameObject vfx = Instantiate(hitVFXPrefab, spawnPos, spawnRot);

                if (parentVFXToHit)
                    vfx.transform.SetParent(collision.collider.transform, true);

                Destroy(vfx, hitVFXLifetime);
            }
        }
    }

    public void StopRetracting()
    {
        isRetracting = false;
    }

    public bool IsHeld() { return isHeld; }

    public float DistanceFromHand() { return Vector3.Distance(hand.position, transform.position); }
}
