using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleInputs : MonoBehaviour
{
    public PlayerInput playerInput;

    private Vector2 move, look;
    private float isThrowing, isCatching, isRunning, isJumping, isDashing;

    // --- Ataque ---
    private float attackStartTime;
    private float holdThreshold = 0.4f;

    private bool attackTapped;
    private bool holdReleased;
    private bool attackHeld;

    // --- Jump ---
    private bool normalJumpPressed;
    private bool isChargingJump;
    private bool jumpReleased;
    private bool jumpStartedAsCharge;
    private bool jumpHeld;

    public void OnMove(InputAction.CallbackContext context)
        => move = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context)
        => look = context.ReadValue<Vector2>();

    public void OnThrowing(InputAction.CallbackContext context)
        => isThrowing = context.ReadValue<float>();

    public void OnCatching(InputAction.CallbackContext context)
        => isCatching = context.ReadValue<float>();

    public void OnRunning(InputAction.CallbackContext context)
        => isRunning = context.ReadValue<float>();

    public void OnDash(InputAction.CallbackContext context)
        => isDashing = context.ReadValue<float>();

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            attackHeld = true;
            attackStartTime = Time.time;
        }
        else if (context.canceled)
        {
            attackHeld = false;

            float heldTime = Time.time - attackStartTime;

            if (heldTime < holdThreshold)
                attackTapped = true;
            else
                holdReleased = true;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpHeld = true;

            // Cada press empieza limpio.
            jumpReleased = false;
            normalJumpPressed = false;

            if (IsRunning())
            {
                // El modo se decide acá y no vuelve a cambiar hasta soltar Jump.
                jumpStartedAsCharge = true;
                isChargingJump = true;

                // Un charged jump nunca debe dejar activo el salto normal.
                isJumping = 0f;
            }
            else
            {
                jumpStartedAsCharge = false;
                isChargingJump = false;

                // Edge de salto normal.
                isJumping = 1f;
                normalJumpPressed = true;
            }

            return;
        }

        if (context.canceled)
        {
            jumpHeld = false;

            // SIEMPRE limpiar el salto normal.
            // Antes esto dependía del estado actual de Shift y podía quedar en 1 para siempre.
            isJumping = 0f;
            normalJumpPressed = false;

            if (jumpStartedAsCharge)
                jumpReleased = true;

            isChargingJump = false;
            jumpStartedAsCharge = false;
        }
    }

    public bool TryConsumeTap()
    {
        if (!attackTapped)
            return false;

        attackTapped = false;
        return true;
    }

    public bool TryConsumeHoldReleased()
    {
        if (!holdReleased)
            return false;

        holdReleased = false;
        return true;
    }

    public bool IsAttackHeld() => attackHeld;

    public void ResetAttackFlags()
    {
        attackHeld = false;
        attackTapped = false;
        holdReleased = false;
    }

    // Salto normal: sólo una vez por press.
    public bool TryConsumeJumpPressed()
    {
        if (!normalJumpPressed)
            return false;

        normalJumpPressed = false;
        return true;
    }

    public bool IsChargingJump() => isChargingJump;

    public bool JumpReleased() => jumpReleased;

    public void ConsumeJumpReleased()
        => jumpReleased = false;

    // Se llama cuando el charged jump termina o es interrumpido.
    public void FinishChargedJumpInput()
    {
        isChargingJump = false;
        jumpStartedAsCharge = false;
        jumpReleased = false;

        isJumping = 0f;
        normalJumpPressed = false;
    }

    public void ResetJumpFlags()
    {
        isJumping = 0f;
        normalJumpPressed = false;
        isChargingJump = false;
        jumpReleased = false;
        jumpStartedAsCharge = false;
        jumpHeld = false;
    }

    public bool IsJumpHeld() => jumpHeld;

    // Legacy, por compatibilidad con cualquier script externo.
    public bool IsJumping() => isJumping > 0.5f;

    public Vector2 GetMoveVector2() => move;
    public Vector2 GetLookVector2() => look;

    public bool IsThrowing() => isThrowing > 0.5f;
    public bool IsCatching() => isCatching > 0.5f;
    public bool IsRunning() => isRunning > 0.5f;
    public bool IsDashing() => isDashing > 0.5f;

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            ResetJumpFlags();
            playerInput.DeactivateInput();
        }
        else
        {
            playerInput.ActivateInput();
        }
    }

    private void OnDisable()
    {
        ResetJumpFlags();
        ResetAttackFlags();

        isThrowing = 0f;
        isCatching = 0f;
        isRunning = 0f;
        isDashing = 0f;
    }
}
