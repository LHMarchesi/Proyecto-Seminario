using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;
    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;
        transform.forward = cam.transform.forward;
    }
}
