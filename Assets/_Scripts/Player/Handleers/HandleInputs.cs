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

    // --- Jump charge ---
    private bool isChargingJump;
    private bool jumpReleased;
    private bool holdReleased;
    private bool attackHeld;

    public void OnMove(InputAction.CallbackContext context) => move = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => look = context.ReadValue<Vector2>();
    public void OnThrowing(InputAction.CallbackContext context) => isThrowing = context.ReadValue<float>();
    public void OnCatching(InputAction.CallbackContext context) => isCatching = context.ReadValue<float>();
    public void OnRunning(InputAction.CallbackContext context) => isRunning = context.ReadValue<float>();
    public void OnDash(InputAction.CallbackContext context) => isDashing = context.ReadValue<float>();

    // --- Ataque ---
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
            {
                attackTapped = true;
            }
            else
            {
                holdReleased = true;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsRunning())
        {
            isJumping = context.ReadValue<float>();
        }
        else
        {
            if (context.started)
            {
                isChargingJump = true;
            }
            else if (context.canceled)
            {
                isChargingJump = false;
                jumpReleased = true;
            }
        }
        // (opcional) si quer detectar performed para salto inmediato, pos manejarlo aqu
    }



    // --- Métodos de consumo ---
    public bool TryConsumeTap()
    {
        if (!attackTapped) return false;
        attackTapped = false;
        return true;
    }

    public bool TryConsumeHoldReleased()
    {
        if (!holdReleased) return false;
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

    // Jump charge
    public bool IsChargingJump() => isChargingJump;

    // devuelve true una vez cuando se consumió el release del salto
    public bool JumpReleased()                          //////////
    {
        if (jumpReleased)
        {
            return true;
        }
        return false;
    }


    // El estado que maneja el release debe llamar esto para consumir el flag
    public void ConsumeJumpReleased() => jumpReleased = false;

    // --- Otros getters ---
    public Vector2 GetMoveVector2() => move;
    public Vector2 GetLookVector2() => look;

    public bool IsThrowing() => isThrowing == 1f;
    public bool IsCatching() => isCatching == 1f;
    public bool IsRunning() => isRunning == 1f;
    public bool IsJumping() => isJumping == 1f;
    public bool IsDashing() => isDashing == 1f;

    public void SetPaused(bool paused)
    {
        if (paused)
            playerInput.DeactivateInput();
        else
            playerInput.ActivateInput();
    }
}
