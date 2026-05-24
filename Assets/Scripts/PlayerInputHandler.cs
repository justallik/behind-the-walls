using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool SprintInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool BlockInput { get; private set; }
    public bool DodgeInput { get; private set; }
    public bool SuperAttackInput { get; private set; }

    private Keyboard keyboard;
    private Mouse mouse;

    private void Update()
    {
        if (keyboard == null) keyboard = Keyboard.current;
        if (mouse == null) mouse = Mouse.current;
        
        HandleMoveInput();
        HandleSprintInput();
        HandleCrouchInput();
        HandleLookInput();
        HandleCombatInput();
    }

    private void HandleMoveInput()
    {
        Vector2 moveInput = Vector2.zero;
        
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        }
        
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
        
        MoveInput = moveInput;
    }

    private void HandleSprintInput()
    {
        SprintInput = keyboard != null && keyboard.leftShiftKey.isPressed;
    }

    private void HandleCrouchInput()
    {
        CrouchInput = keyboard != null && keyboard.leftCtrlKey.isPressed;
    }

    private void HandleLookInput()
    {
        if (mouse != null)
            LookInput = mouse.delta.ReadValue();
    }

    private void HandleCombatInput()
    {
        if (mouse != null)
        {
            AttackInput = mouse.leftButton.wasPressedThisFrame;
            BlockInput = mouse.rightButton.isPressed;
        }

        if (keyboard != null)
        {
            DodgeInput = keyboard.leftShiftKey.isPressed && keyboard.sKey.wasPressedThisFrame;
            SuperAttackInput = keyboard.vKey.wasPressedThisFrame;
        }
    }
}