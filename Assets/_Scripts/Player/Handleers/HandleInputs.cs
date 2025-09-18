using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class HandleInputs : MonoBehaviour
{
    private Vector2 move, look;
    private float isThrowing, isCatching, isRunning, isJumping, isDashing;

    // --- Ataque ---
    private float attackStartTime;
    private float holdThreshold = 0.4f;

    private bool attackTapped;
    private bool isChargingAttack;
    private bool attackReleased;

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

    // --- Jump: carga en started, release en canceled ---
    public void OnJump(InputAction.CallbackContext context)
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
        // (opcional) si querés detectar performed para salto inmediato, podés manejarlo aquí
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
    public float GetHeldTime() => attackHeld ? Time.time - attackStartTime : 0f;

    public void ResetAttackFlags()
    {
        attackHeld = false;
        attackTapped = false;
        holdReleased = false;
    }

    // Jump charge
    public bool IsChargingJump() => isChargingJump;

    // devuelve true una vez cuando se consumió el release del salto
    public bool JumpReleased()
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
}
