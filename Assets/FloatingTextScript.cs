using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextScript : MonoBehaviour
{
    public float DestroyTime = 2f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public Vector3 RandomizeOffset = new Vector3(1f, 0, 0);
    public Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.transform;
        Destroy(gameObject, DestroyTime);

        transform.localPosition += offset;
        transform.LookAt(player);
        transform.localPosition += new Vector3(Random.Range(-RandomizeOffset.x, RandomizeOffset.x), Random.Range(-RandomizeOffset.y, RandomizeOffset.y), Random.Range(-RandomizeOffset.z, RandomizeOffset.z));
    }
}
