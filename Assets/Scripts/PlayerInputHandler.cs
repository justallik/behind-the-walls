using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обработка ввода от игрока. Отделена от логики движения для чистоты кода.
/// </summary>
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

    // ⚡ ОПТИМИЗАЦИЯ: кэшируем референции на Keyboard и Mouse
    private Keyboard keyboard;
    private Mouse mouse;

    private void Update()
    {
        // ⚡ ОПТИМИЗАЦИЯ: кэшируем на первые кадры
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
        // WASD или стики
        Vector2 moveInput = Vector2.zero;
        
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        }
        
        // Нормализуем диагональ
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
        
        MoveInput = moveInput;
    }

    private void HandleSprintInput()
    {
        // Left Shift для спринта (удержание)
        SprintInput = keyboard != null && keyboard.leftShiftKey.isPressed;
    }

    private void HandleCrouchInput()
    {
        // Left Ctrl для приседа (удержание)
        CrouchInput = keyboard != null && keyboard.leftCtrlKey.isPressed;
    }

    private void HandleLookInput()
    {
        // Мышь для поворота (обработать в отдельном скрипте камеры)
        if (mouse != null)
        {
            LookInput = mouse.delta.ReadValue();
        }
    }

    private void HandleCombatInput()
    {
        if (mouse != null)
        {
            AttackInput = mouse.leftButton.wasPressedThisFrame;
            BlockInput = mouse.rightButton.isPressed; // Удержание для блока
        }

        // Уклонение: проверяем нажатие S пока зажат Shift
        if (keyboard != null)
        {
            DodgeInput = keyboard.leftShiftKey.isPressed && keyboard.sKey.wasPressedThisFrame;
            SuperAttackInput = keyboard.vKey.wasPressedThisFrame; // V для супер-удара
        }
    }
}
