using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePowerUp : MonoBehaviour
{
    public PlayerController player;
    public HandleAttack playerAttack;

    private void Start()
    {
        if(playerAttack == null)
        {
            playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<HandleAttack>();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            playerAttack.attackDamage += Random.Range(1, 10);
        }
    }
}
