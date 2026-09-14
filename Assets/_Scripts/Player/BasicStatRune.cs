using UnityEngine;

public enum BasicRuneStat
{
    MaxHealth,
    MeleeDamage,
    MoveSpeed,
    MaxJumpForce
}

[DisallowMultipleComponent]
public class BasicStatRune : MonoBehaviour
{
    [Header("Rune")]
    [SerializeField] private BasicRuneStat stat;
    [SerializeField, Min(0f)] private float amount = 1f;

    [Header("Pickup")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject pickupVFX;
    [SerializeField, Min(0f)] private float pickupVFXLifetime = 2f;
    [SerializeField] private string pickupSound = "";

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private bool bob = true;
    [SerializeField, Min(0f)] private float bobHeight = 0.15f;
    [SerializeField, Min(0.01f)] private float bobSpeed = 2f;

    [Header("Diagnostics")]
    [SerializeField] private bool logDebug;

    public BasicRuneStat Stat => stat;
    public float Amount => amount;

    private bool collected;
    private Vector3 visualStartLocalPosition;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        visualStartLocalPosition =
            visualRoot.localPosition;
    }

    private void Update()
    {
        if (collected ||
            visualRoot == null)
        {
            return;
        }

        if (rotate)
        {
            visualRoot.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime,
                Space.Self);
        }

        if (bob)
        {
            Vector3 position =
                visualStartLocalPosition;

            position.y +=
                Mathf.Sin(
                    Time.time *
                    bobSpeed) *
                bobHeight;

            visualRoot.localPosition =
                position;
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (collected ||
            other == null)
        {
            return;
        }

        PlayerController player =
            other.GetComponentInParent<
                PlayerController>();

        if (player == null)
            return;

        if (!string.IsNullOrEmpty(playerTag) &&
            !other.CompareTag(playerTag) &&
            !player.CompareTag(playerTag))
        {
            return;
        }

        Collect(player);
    }

    private void Collect(
        PlayerController player)
    {
        if (collected ||
            player == null)
        {
            return;
        }

        collected = true;

        ApplyStat(player);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRunePickup(
                stat,
                amount);
        }

        SpawnFeedback();

        if (logDebug)
        {
            Debug.Log(
                "[BasicStatRune] " +
                stat +
                " +" +
                amount +
                " recogida.",
                this);
        }

        Destroy(gameObject);
    }

    private void ApplyStat(
        PlayerController player)
    {
        switch (stat)
        {
            case BasicRuneStat.MaxHealth:
                player.AddMaxHealth(
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(amount)));
                break;

            case BasicRuneStat.MeleeDamage:
                player.AddMaxDamage(amount);
                break;

            case BasicRuneStat.MoveSpeed:
                player.AddMoveSpeed(amount);
                break;

            case BasicRuneStat.MaxJumpForce:
                player.AddMaxJumpForce(amount);
                break;
        }
    }

    private void SpawnFeedback()
    {
        if (pickupVFX != null)
        {
            GameObject vfx =
                Instantiate(
                    pickupVFX,
                    transform.position,
                    transform.rotation);

            if (pickupVFXLifetime > 0f)
                Destroy(
                    vfx,
                    pickupVFXLifetime);
        }

        if (!string.IsNullOrWhiteSpace(
                pickupSound) &&
            SoundManagerOcta.Instance != null)
        {
            SoundManagerOcta.Instance
                .PlaySound(
                    pickupSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider col =
            GetComponent<Collider>();

        if (col == null)
            return;

        Gizmos.DrawWireCube(
            col.bounds.center,
            col.bounds.size);
    }
}
