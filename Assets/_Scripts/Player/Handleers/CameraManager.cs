using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        // Ensure there's only one instance of GameManager (Singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void DoScreenShake(float duration, float magnitude)
    {
        StartCoroutine(ScreenShake(duration, magnitude));
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}
