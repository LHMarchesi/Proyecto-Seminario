using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleInputs : MonoBehaviour
{
    private Vector2 move, look;
    private float isThrowing, isCatching, isAttacking;
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


    public Vector2 GetMoveVector2() { return move; }  // Return public values
    public Vector2 GetLookVector2() { return look; }

    public bool IsThrowing() { return isThrowing == 1f; }

    public bool IsCatching() { return isCatching == 1f; }

    public bool IsAttacking() { return isAttacking == 1f; }

}
