using System.Collections;
using UnityEngine;

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
    private float playerSpeed;

    private Coroutine hitStopRoutine;

    private void Awake()
    {
        
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
    }

    public void Attack(float damage,float radius, float shakeDuration, float shakeMagnitude, float kickPitch, float kickYaw, float hitStopDuration)
    {
        if (!readyToAttack || attacking) return;

        StartCoroutine(DoAttack(damage, radius, shakeDuration, shakeMagnitude, kickPitch, kickYaw, hitStopDuration));
    }

    public void PlayHitAttackSound()
    {
        string[] attackSounds = {"AttackHit2", "AttackHit3" };

        int index = Random.Range(0, attackSounds.Length);

        SoundManagerOcta.Instance.PlaySound(attackSounds[index]);
    }

    public void PlayAttackSound()
    {
        string[] attackSounds = { "Attack2", "Attack3" };

        int index = Random.Range(0, attackSounds.Length);

        SoundManagerOcta.Instance.PlaySound(attackSounds[index]);
    }

    private IEnumerator DoAttack(float damage, float radius, float shakeDuration, float shakeMagnitude, float kickPitch, float kickYaw, float hitStopDuration)
    {
        readyToAttack = false;
        attacking = true;
        bool hitSomething = false;

        PlayAttackSound();


        playerSpeed = playerContext.PlayerController.currentSpeed;
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.currentSpeed - playerContext.PlayerController.playerStats.speedReductor);

        yield return new WaitForSecondsRealtime(attackDelay);

        Vector3 origin = Camera.main.transform.position + Camera.main.transform.forward * (attackDistance * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, radius, attackLayer);

        foreach (var hit in hits)
        {
            IDamageable damagable = hit.GetComponent<IDamageable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
                HitTarget(hit.ClosestPoint(origin));
                hitSomething = true;
            }
        }

        if (hitSomething) {
            CameraManager.Instance.DoScreenShake(
            shakeDuration,
            shakeMagnitude
        );

            CameraManager.Instance.DoCameraKick(
                kickPitch,
                Random.Range(-kickYaw, kickYaw)
            );
            yield return HitStopRoutine(hitStopDuration);
        }

        playerContext.PlayerController.ChangeSpeed(playerSpeed);
        attacking = false;
        readyToAttack = true;
    }

    void HitTarget(Vector3 pos)
    {
        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity); // Instantiate effect
        Destroy(GO, 3);
        PlayHitAttackSound();
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float previousTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = previousTimeScale;

        hitStopRoutine = null;
    }
}
