using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public Transform shakePivot;
    [SerializeField] public Transform kickPivot;

    [Header("Kick")]
    [SerializeField] public float kickReturnSpeed = 12f;

    private Coroutine shakeRoutine;

    private Vector3 currentKick;
    private Vector3 targetKick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        currentKick = Vector3.Lerp(
            currentKick,
            targetKick,
            Time.deltaTime * kickReturnSpeed
        );

        targetKick = Vector3.Lerp(
            targetKick,
            Vector3.zero,
            Time.deltaTime * kickReturnSpeed
        );

        kickPivot.localRotation = Quaternion.Euler(currentKick);
    }

    public void DoCameraKick(float pitch, float yaw)
    {
        targetKick += new Vector3(-pitch, yaw, 0f);
    }

    public void DoScreenShake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ScreenShake(duration, magnitude));
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakePivot.localPosition = new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        shakePivot.localPosition = Vector3.zero;
        shakeRoutine = null;
    }
}
