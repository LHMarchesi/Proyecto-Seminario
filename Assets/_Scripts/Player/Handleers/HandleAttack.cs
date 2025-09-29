using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class HandleAttack : MonoBehaviour
{
    
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
        
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
    }

    public void Attack(float damage,float radius, float shakeDuration, float shakeMagnitude)
    {
        if (!readyToAttack || attacking) return;

        StartCoroutine(DoAttack(damage, radius, shakeDuration, shakeMagnitude));
    }

    private IEnumerator DoAttack(float damage, float radius, float shakeDuration, float shakeMagnitude)
    {
        readyToAttack = false;
        attacking = true;

        SoundManager.Instance.PlaySFX(swordSwing);
        

        playerSpeed = playerContext.PlayerController.currentSpeed;
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.currentSpeed - playerContext.PlayerController.playerStats.speedReductor);

        yield return new WaitForSecondsRealtime(attackDelay);

        Vector3 origin = Camera.main.transform.position + Camera.main.transform.forward * (attackDistance * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, radius, attackLayer);

        foreach (var hit in hits)
        {
            HitTarget(hit.ClosestPoint(origin));
            IDamageable damagable = hit.GetComponent<IDamageable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
                CameraManager.Instance.DoScreenShake(shakeDuration, shakeMagnitude);
            }
        }

        playerContext.PlayerController.ChangeSpeed(playerSpeed);
        attacking = false;
        readyToAttack = true;
    }

    void HitTarget(Vector3 pos)
    {
        SoundManager.Instance.PlaySFX(hitSound);

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
