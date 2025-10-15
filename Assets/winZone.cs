using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class winZone : MonoBehaviour
{
    public float rotationSpeed = 50f; // Degrees per second
    private void OnCollisionEnter(Collision collision)
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("MainMenu");
    }


    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
