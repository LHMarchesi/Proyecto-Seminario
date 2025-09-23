using UnityEngine;

public class winZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.Win();
    }
}
