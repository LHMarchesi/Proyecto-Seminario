using TMPro;
using UnityEngine;

public class DamageNumberFeedback : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.7f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float fadeStartPercent = 0.5f;

    [Header("Billboard")]
    [SerializeField] private Vector3 rotationOffset;

    private Camera mainCamera;
    private TMP_Text text;

    private float elapsedTime;
    private Color originalColor;

    private void Awake()
    {
        mainCamera = Camera.main;

        text = GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            originalColor = text.color;
        }
    }

    private void Update()
    {
        // ==========================================
        // SUBIR
        // ==========================================

        transform.position +=
            Vector3.up *
            moveSpeed *
            Time.deltaTime;

        // ==========================================
        // TIEMPO
        // ==========================================

        elapsedTime += Time.deltaTime;

        // ==========================================
        // FADE
        // ==========================================

        if (text != null)
        {
            float fadeStartTime =
                lifetime * fadeStartPercent;

            if (elapsedTime >= fadeStartTime)
            {
                float fadeDuration =
                    lifetime - fadeStartTime;

                float fadeProgress =
                    (elapsedTime - fadeStartTime)
                    / fadeDuration;

                Color color = text.color;

                color.a = Mathf.Lerp(
                    originalColor.a,
                    0f,
                    fadeProgress
                );

                text.color = color;
            }
        }

        // ==========================================
        // DESTROY
        // ==========================================

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        // ==========================================
        // MIRAR A LA CAMARA
        // ==========================================

        Vector3 direction =
            transform.position -
            mainCamera.transform.position;

        transform.rotation =
            Quaternion.LookRotation(direction);

        transform.Rotate(
            rotationOffset,
            Space.Self
        );
    }
}