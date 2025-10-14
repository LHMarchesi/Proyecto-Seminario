using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainFollowScript : MonoBehaviour
{
    public float rainSmoothSpeed = .01f;
    public Transform player;
    Vector3 currentPlayerPos;
    private Vector3 velocity = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        //also setting y to 15 to keep it above player (its rain)
        currentPlayerPos = new Vector3(player.localPosition.x - .5f, 1f, player.localPosition.z);

        //do da smoothdamp to player pos
        transform.position = Vector3.SmoothDamp(transform.position, currentPlayerPos, ref velocity, rainSmoothSpeed);
    }
}
