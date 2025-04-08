using UnityEngine;

public class HandleAttack : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerContext playerContext;

    [Header("Attacking")]
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackSpeed = 1f;
    //  [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;


    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
        if (playerContext.handleInputs.IsAttacking())
            Attack();
    }

    public void Attack() // Attack using forward RayCast
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);  // Play swing
        audioSource.PlayOneShot(swordSwing);
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast() //  RayCast
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity); // Instantiate effect
        Destroy(GO, 10);
    }
}
