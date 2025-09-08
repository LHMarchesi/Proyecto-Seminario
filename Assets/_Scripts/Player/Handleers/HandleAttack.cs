using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class HandleAttack : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerContext playerContext;

    [Header("Attacking")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackSpeed;
    [SerializeField] public int attackDamage;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;


    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;
    private float playerSpeed;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
        if (playerContext.HandleInputs.IsAttacking())
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

        playerSpeed = playerContext.PlayerController.currentSpeed;
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.currentSpeed / 2); // Reduce speed while attacking
    }

    void ResetAttack()
    {
        playerContext.PlayerController.ChangeSpeed(playerSpeed); // Restore speed
        attacking = false;
        readyToAttack = true;
    }


    void AttackRaycast()
    {
        Vector3 origin = Camera.main.transform.position + Camera.main.transform.forward * (attackDistance * 0.5f);

        Collider[] hits = Physics.OverlapSphere(origin, 2.5f, attackLayer); // radio de 1 metro
        foreach (var hit in hits)
        {
            HitTarget(hit.ClosestPoint(origin));
            IDamageable damagable = hit.GetComponent<IDamageable>();
            if (damagable != null)
            {
                damagable.TakeDamage(attackDamage);

           //     StartCoroutine(HitStop(0.000f, hit.gameObject));
                CameraManager.Instance.DoScreenShake(0.1f, 0.1f);
            }
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity); // Instantiate effect
        Destroy(GO, 3);
    }

    private IEnumerator HitStop(float duration, GameObject enemy)
    {
        float originalPlayerSpeed = playerContext.PlayerController.currentSpeed;
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();

        playerContext.PlayerController.ChangeSpeed(0);

        Vector3 enemyVel = Vector3.zero;
        if (enemyRb != null)
        {
            enemyVel = enemyRb.velocity;
            enemyRb.isKinematic = true;
        }

        yield return new WaitForSecondsRealtime(duration);

        playerContext.PlayerController.ChangeSpeed(originalPlayerSpeed);
        if (enemyRb != null)
        {
            enemyRb.isKinematic = false;
            enemyRb.velocity = enemyVel;
        }
    }
}
