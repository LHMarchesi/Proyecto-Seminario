using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScriptHoming : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float force;
    [SerializeField] private float rotationForce;
    [SerializeField] private float distance;
    [SerializeField] private float distanceToTarget;
    private Rigidbody rb;
    public GameObject FindClosestByTag(string Enemy)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(Enemy);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;

            }
        }
        Debug.Log(closest.name);
        return closest;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Debug.Log("DISTANCE ES" + distance);
        target = FindClosestByTag("Enemy");
        distance = Vector3.Distance(transform.position, target.GetComponent<Transform>().position);
        if (distance < distanceToTarget)
        {
            Vector3 direction = target.GetComponent<Transform>().position - rb.position;
            direction.Normalize();
            Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);
            rb.angularVelocity = rotationAmount * rotationForce;
            rb.velocity = transform.forward * rotationForce;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanceToTarget);
    }
}
