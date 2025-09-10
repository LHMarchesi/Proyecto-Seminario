using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleInputs : MonoBehaviour
{
    private Vector2 move, look;
    private float isThrowing, isCatching, isAttacking, isRunning, isJumping, isDashing;
    private bool isChargingJump, jumpReleased;
    private bool attackRealed;
    private bool isChragingAttack;

    public void OnMove(InputAction.CallbackContext context) // Catch player input
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context) // Catch mouse input
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnThrowing(InputAction.CallbackContext context) // Catch Throw input
    {
        isThrowing = context.ReadValue<float>();
    }
    public void OnCatching(InputAction.CallbackContext context) // Catch catch input
    {
        isCatching = context.ReadValue<float>();
    }

    public void OnAttack(InputAction.CallbackContext context) // Catch attack input
    {
        isAttacking = context.ReadValue<float>();
    }

    public void OnRunning(InputAction.CallbackContext context) // Catch run input
    {
        isRunning = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            isChargingJump = true;
        else if (context.canceled)
        {
            jumpReleased = true; // marcamos release
            isChargingJump = false;
        }
    }

    public void OnChargeAttack(InputAction.CallbackContext context)
    {
        if (context.started)
            isChragingAttack = true;
        else if (context.canceled)
        {
            attackRealed = true; // marcamos release
            isChragingAttack = false;
        }
    }

    public bool IsChargingJump() => isChargingJump;
    public bool IsChragingAttack() => isChragingAttack;
    public bool JumpReleased()
    {
        return jumpReleased;
    }
    public bool AttackRealed()
    {
        return attackRealed;
    }

    // Consumir el release, llamado por la máquina de estados
    public void ConsumeJumpReleased()
    {
        jumpReleased = false;
    }
    public void OnDash(InputAction.CallbackContext context) // Catch run input
    {
        isDashing = context.ReadValue<float>();
    }

    public Vector2 GetMoveVector2() { return move; }  // Return public values
    public Vector2 GetLookVector2() { return look; }

    public bool IsThrowing() { return isThrowing == 1f; }

    public bool IsCatching() { return isCatching == 1f; }

    public bool IsAttacking() { return isAttacking == 1f; }

    public bool IsRunning() { return isRunning == 1f; }

    public bool IsJumping() { return isJumping == 1f; }

    public bool IsDashing() { return isDashing == 1f; }

}
