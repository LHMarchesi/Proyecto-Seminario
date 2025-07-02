using UnityEngine;
using UnityEngine.InputSystem.XR;

public class HandleHeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;


    [Header("Bobbing Settings")]
    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float walkBobAmount;
    [SerializeField] private float runBobSpeed;
    [SerializeField] private float runBobAmount;

    [Header("FOV Settings")]
    [SerializeField] private float baseFOV;
    [SerializeField] private float runFOV;
    [SerializeField] private float fovTransitionSpeed;

    private PlayerContext playerContext;
    private Vector3 initialPosition;
    private float bobTimer;
    private Camera mainCamera;

    void Start()
    {
        playerContext = GetComponent<PlayerContext>();
        initialPosition = cameraTransform.localPosition;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector2 input = playerContext.HandleInputs.GetMoveVector2();
        bool isGrounded = playerContext.PlayerController.IsGrounded();
        bool isMoving = input.magnitude > 0.1f && isGrounded;

        bool isRunning = playerContext.HandleInputs.IsRunning(); 

        float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
        float bobAmount = isRunning ? runBobAmount : walkBobAmount;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;
            float bobY = Mathf.Sin(bobTimer) * bobAmount;

            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialPosition + new Vector3(bobX, bobY, 0f), Time.deltaTime * 5f);
        }
        else
        {
            bobTimer = 0;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialPosition, Time.deltaTime * 5f);
        }

        float targetFOV = isRunning ? runFOV : baseFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }
    
}
