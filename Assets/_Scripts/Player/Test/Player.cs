using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerChar playerCharacter;
    [SerializeField] private PlayerCam playerCamera;

    private PlayerInputActions inputActions;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        inputActions = new PlayerInputActions();
        inputActions.Enable();
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        var input = inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPerformedThisFrame()
             ?CrouchInput.Toggle
             :CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
    }

    void LateUpdate()
    {
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
    }
}
