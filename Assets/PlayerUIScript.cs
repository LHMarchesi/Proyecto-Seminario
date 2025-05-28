using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIScript : MonoBehaviour
{
    public TextMeshProUGUI attackStat;
    public TextMeshProUGUI speedStat;
    public PlayerController player;
    public HandleAttack playerAttack;

    private void Update()
    {
        attackStat.text = "Attack Stat:" + playerAttack.attackDamage;
        speedStat.text = "Speed Stat:" + player.WalkingSpeed;
    }
}
