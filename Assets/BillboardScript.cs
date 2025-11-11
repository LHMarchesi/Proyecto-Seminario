using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardScript : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera for performance
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the object face the camera
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
