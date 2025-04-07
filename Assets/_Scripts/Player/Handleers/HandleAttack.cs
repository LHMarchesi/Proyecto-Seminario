using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class HandleAttack : MonoBehaviour
{
    AudioSource audioSource;
    HandleInputs handleInputs;

    [Header("Attacking")]
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackSpeed = 1f;
    //  [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip hitSound;

    [Header("Debug")]
    [SerializeField] bool attacking = false;
    [SerializeField] bool readyToAttack = true;
    [SerializeField] int attackCount;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        handleInputs = GetComponent<HandleInputs>();
    }

    private void Update()
    {
        if (handleInputs.IsAttacking())
            Attack();
    }

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        // audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 10);
    }

    public bool isAttacking()
    {
        return attacking;
    }
}
