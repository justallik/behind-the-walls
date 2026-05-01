using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("═══ СКОРОСТИ ═══")]
    [SerializeField] private float normalSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float speedChangeRate = 10f;
    
    [Header("═══ ПРИСЕД ═══")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private Transform visualCrouchRoot;
    [SerializeField] private float crouchVisualScaleY = 0.6f;
    
    [Header("═══ ГРАВИТАЦИЯ ═══")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float terminalVelocity = -53f;
    
    [Header("═══ ПРОВЕРКА ЗЕМЛИ ═══")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    
    [Header("═══ ВЫНОСЛИВОСТЬ ═══")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 10f;           // Убывание за 10 сек
    [SerializeField] private float staminaRecoveryWalk = 3.33f;      // Восстановление за 30 сек при ходьбе
    [SerializeField] private float staminaRecoveryIdle = 5f;         // Восстановление за 20 сек стоя
    [SerializeField] private float regenDelay = 1.0f;
    [SerializeField] private AnimationCurve staminaDrainCurve = AnimationCurve.Linear(0, 1, 1, 0.5f); // Кривая убывания стамины
    private float regenTimer = 0f;
    
    [Header("═══ НАСТРОЙКИ УКЛОНЕНИЯ (СПРИНТ 4) ═══")]
    [SerializeField] private float dodgeForce = 12f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeRecoveryTime = 0.15f; // Стоп-момент
    
    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    
    private float currentSpeed = 0f;
    private Vector3 velocity = Vector3.zero;
    private float currentStamina;
    
    private bool isGrounded;
    private bool isCrouching;
    private bool canSprint = true;
    private bool isDodging = false; // Блокировка движения
    
    private Vector3 standingCenterPos;
    private Vector3 standingVisualScale;
    private Vector3 standingVisualPos; // 🔥 Запоминаем исходную позицию модели
    
    // ⚡ ОПТИМИЗАЦИЯ: кэшируем позицию для CheckSphere
    private Vector3 cachedSpherePosition;
    private const float MOVE_INPUT_THRESHOLD_SQ = 0.01f; // Квадрат 0.1

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        
        if (controller != null)
        {
            normalHeight = controller.height;
            standingCenterPos = controller.center;
        }
        
        if (visualCrouchRoot != null)
        {
            standingVisualScale = visualCrouchRoot.localScale;
            standingVisualPos = visualCrouchRoot.localPosition; // Запоминаем
        }
        
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (controller == null || inputHandler == null || groundCheck == null) 
            return;

        // � БЛОКИРОВКА ВВОДА ЕСЛИ ИДЁТ INTRO
        if (WakeUpSceneController.introPlaying)
        {
            ApplyGravity();
            controller.Move(new Vector3(0, velocity.y * Time.deltaTime, 0));
            return;
        }

        // �🛑 ЕСЛИ УКЛОНЯЕМСЯ - БЛОКИРУЕМ ОБЫЧНОЕ УПРАВЛЕНИЕ
        if (isDodging)
        {
            ApplyGravity();
            controller.Move(new Vector3(0, velocity.y * Time.deltaTime, 0));
            return; 
        }

        GroundedCheck();
        UpdateStamina();
        ApplyGravity();
        HandleCrouch();
        Move();
    }

    private void GroundedCheck()
    {
        // ⚡ ОПТИМИЗАЦИЯ: используем кэшированную позицию вместо создания новой Vector3
        cachedSpherePosition.x = transform.position.x;
        cachedSpherePosition.y = transform.position.y - groundedOffset;
        cachedSpherePosition.z = transform.position.z;
        isGrounded = Physics.CheckSphere(cachedSpherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void UpdateStamina()
    {
        // ⚡ ОПТИМИЗАЦИЯ: используем sqrMagnitude вместо magnitude (избегаем Sqrt)
        bool isMoving = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ;
        bool isSprinting = inputHandler.SprintInput && !isCrouching && currentSpeed > 0;
        
        if (regenTimer > 0) regenTimer -= Time.deltaTime;

        if (isSprinting && currentStamina > 0)
        {
            // 📉 Используем кривую для изогнутого убывания стамины
            float normalizedStamina = currentStamina / maxStamina; // 0 до 1
            float curveMultiplier = staminaDrainCurve.Evaluate(normalizedStamina); // Получаем множитель из кривой
            float drainThisFrame = staminaDrainRate * curveMultiplier * Time.deltaTime;
            currentStamina -= drainThisFrame;
            
            // 🔍 DEBUG
            Debug.Log($"🏃 СПРИНТ: -{ drainThisFrame:F2}/frame (Drain: {staminaDrainRate}, Mult: {curveMultiplier:F2})");
            
            regenTimer = regenDelay;
            if (currentStamina <= 0) { currentStamina = 0; canSprint = false; }
        }
        else if (currentStamina < maxStamina && regenTimer <= 0)
        {
            float regenThisFrame = 0f;
            if (!isMoving)
            {
                regenThisFrame = staminaRecoveryIdle * Time.deltaTime;
                Debug.Log($"😴 СТОЯ: +{regenThisFrame:F2}/frame (Regen: {staminaRecoveryIdle})");
            }
            else if (isMoving && !isSprinting)
            {
                regenThisFrame = staminaRecoveryWalk * Time.deltaTime;
                Debug.Log($"🚶 ХОДЬБА: +{regenThisFrame:F2}/frame (Regen: {staminaRecoveryWalk})");
            }
            currentStamina += regenThisFrame;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        if (currentStamina > maxStamina * 0.5f) canSprint = true;
    }

    private void ApplyGravity()
    {
        if (isGrounded) { if (velocity.y < 0) velocity.y = -2f; }
        else { if (velocity.y < terminalVelocity) velocity.y = terminalVelocity; else velocity.y += gravity * Time.deltaTime; }
    }

    private void HandleCrouch()
    {
        bool wantCrouch = inputHandler.CrouchInput;
        if (wantCrouch != isCrouching)
        {
            isCrouching = wantCrouch;
            if (isCrouching)
            {
                controller.height = crouchHeight;
                // 🔥 Высчитываем правильный центр, чтобы ноги не отрывались от пола
                float newCenterY = standingCenterPos.y - (normalHeight - crouchHeight) * 0.5f;
                controller.center = new Vector3(standingCenterPos.x, newCenterY, standingCenterPos.z);
                
                if (visualCrouchRoot != null) 
                {
                    visualCrouchRoot.localScale = new Vector3(standingVisualScale.x, standingVisualScale.y * crouchVisualScaleY, standingVisualScale.z);
                    // 🔥 Сдвигаем саму модельку вниз
                    visualCrouchRoot.localPosition = standingVisualPos + new Vector3(0, -(normalHeight - crouchHeight) * 0.5f, 0);
                }
            }
            else
            {
                controller.height = normalHeight;
                controller.center = standingCenterPos;
                if (visualCrouchRoot != null) 
                {
                    visualCrouchRoot.localScale = standingVisualScale;
                    visualCrouchRoot.localPosition = standingVisualPos;
                }
            }
        }
    }

    private void Move()
    {
        // ⚡ ОПТИМИЗАЦИЯ: используем sqrMagnitude
        float targetSpeed = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ ? (inputHandler.SprintInput && !isCrouching && canSprint ? sprintSpeed : normalSpeed) : 0f;
        if (isCrouching && targetSpeed > 0) targetSpeed = crouchSpeed;
        
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f) currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        else currentSpeed = targetSpeed;
        
        Vector3 inputDirection = Vector3.zero;
        if (inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ) inputDirection = (transform.right * inputHandler.MoveInput.x + transform.forward * inputHandler.MoveInput.y).normalized;
        
        Vector3 movement = inputDirection * currentSpeed * Time.deltaTime + new Vector3(0, velocity.y * Time.deltaTime, 0);
        controller.Move(movement);
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ---
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsCrouching() => isCrouching;
    public float GetSprintSpeed() => sprintSpeed;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public bool CanSprint() => canSprint;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        regenTimer = regenDelay;
        if (currentStamina <= 0) { currentStamina = 0f; canSprint = false; }
    }

    public void TriggerExhaustion()
    {
        canSprint = false;
        regenTimer = regenDelay;
    }

    // 💨 НОВАЯ ЛОГИКА УКЛОНЕНИЯ С ПРИСЕДОМ
    public void PerformCrouchingDodge(Vector3 sideDirection)
    {
        if (!isDodging) StartCoroutine(DodgeRoutine(sideDirection));
    }

    private IEnumerator DodgeRoutine(Vector3 sideDirection)
    {
        isDodging = true;
        float startTime = Time.time;

        controller.height = crouchHeight;
        float newCenterY = standingCenterPos.y - (normalHeight - crouchHeight) * 0.5f;
        controller.center = new Vector3(standingCenterPos.x, newCenterY, standingCenterPos.z);
        
        if (visualCrouchRoot != null) 
        {
            visualCrouchRoot.localScale = new Vector3(standingVisualScale.x, standingVisualScale.y * crouchVisualScaleY, standingVisualScale.z);
            visualCrouchRoot.localPosition = standingVisualPos + new Vector3(0, -(normalHeight - crouchHeight) * 0.5f, 0);
        }

        while (Time.time < startTime + dodgeDuration)
        {
            controller.Move(sideDirection * dodgeForce * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dodgeRecoveryTime);

        if (!inputHandler.CrouchInput)
        {
            controller.height = normalHeight;
            controller.center = standingCenterPos;
            if (visualCrouchRoot != null) 
            {
                visualCrouchRoot.localScale = standingVisualScale;
                visualCrouchRoot.localPosition = standingVisualPos;
            }
        }

        isDodging = false;
    }
}