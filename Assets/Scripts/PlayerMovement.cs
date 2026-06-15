using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float normalSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float speedChangeRate = 10f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private Transform visualCrouchRoot;
    [SerializeField] private float crouchVisualScaleY = 0.6f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float terminalVelocity = -53f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.5f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 10f;
    [SerializeField] private float staminaRecoveryWalk = 3.33f;
    [SerializeField] private float staminaRecoveryIdle = 5f;
    [SerializeField] private float regenDelay = 1.0f;
    private float regenTimer = 0f;

    [Header("Dodge")]
    [SerializeField] private float dodgeForce = 12f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeRecoveryTime = 0.15f;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;

    private float currentSpeed = 0f;
    private Vector3 velocity = Vector3.zero;
    public float currentStamina;

    private bool isGrounded;
    private bool isCrouching;
    private bool canSprint = true;
    private bool isDodging = false;

    public bool IsSprinting { get; private set; }

    private Vector3 standingCenterPos;
    private Vector3 standingVisualScale;
    private Vector3 standingVisualPos;

    private Vector3 cachedSpherePosition;
    private const float MOVE_INPUT_THRESHOLD_SQ = 0.01f;

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
            standingVisualPos = visualCrouchRoot.localPosition;
        }

        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (controller == null || inputHandler == null || groundCheck == null)
            return;

        if (isDodging)
        {
            ApplyGravity();
            controller.Move(new Vector3(0, velocity.y * Time.deltaTime, 0));
            return;
        }

        GroundedCheck();
        ApplyGravity();
        HandleCrouch();
        Move();
        UpdateStamina();
    }

    private void GroundedCheck()
    {
        cachedSpherePosition.x = transform.position.x;
        cachedSpherePosition.y = transform.position.y - groundedOffset;
        cachedSpherePosition.z = transform.position.z;
        isGrounded = Physics.CheckSphere(cachedSpherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void Move()
    {
        bool wantSprint = inputHandler.SprintInput && !isCrouching && canSprint;
        bool isMoving = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ;

        float targetSpeed = isMoving ? (wantSprint ? sprintSpeed : normalSpeed) : 0f;
        if (isCrouching && targetSpeed > 0) targetSpeed = crouchSpeed;

        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f)
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        else
            currentSpeed = targetSpeed;

        IsSprinting = wantSprint && isMoving && currentSpeed > normalSpeed * 0.9f;

        Vector3 inputDirection = Vector3.zero;
        if (isMoving)
            inputDirection = (transform.right * inputHandler.MoveInput.x + transform.forward * inputHandler.MoveInput.y).normalized;

        controller.Move(inputDirection * currentSpeed * Time.deltaTime + new Vector3(0, velocity.y * Time.deltaTime, 0));
    }

    private void UpdateStamina()
    {
        bool isMoving = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ;

        if (regenTimer > 0) regenTimer -= Time.deltaTime;

        if (IsSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            regenTimer = regenDelay;

            if (currentStamina <= 0.01f)
            {
                currentStamina = 0f;
                canSprint = false;
                IsSprinting = false;
            }
        }
        else if (currentStamina < maxStamina && regenTimer <= 0)
        {
            currentStamina += (!isMoving ? staminaRecoveryIdle : staminaRecoveryWalk) * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            if (!canSprint && currentStamina >= maxStamina * 0.5f)
                canSprint = true;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
        }
        else
        {
            if (velocity.y < terminalVelocity) velocity.y = terminalVelocity;
            else velocity.y += gravity * Time.deltaTime;
        }
    }

    private void HandleCrouch()
    {
        bool wantCrouch = inputHandler.CrouchInput;
        if (wantCrouch == isCrouching) return;

        isCrouching = wantCrouch;
        if (isCrouching)
        {
            controller.height = crouchHeight;
            float newCenterY = standingCenterPos.y - (normalHeight - crouchHeight) * 0.5f;
            controller.center = new Vector3(standingCenterPos.x, newCenterY, standingCenterPos.z);
            if (visualCrouchRoot != null)
            {
                visualCrouchRoot.localScale = new Vector3(standingVisualScale.x, standingVisualScale.y * crouchVisualScaleY, standingVisualScale.z);
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

    // ── Public getters ───────────────────────────────────────────────
    public float GetCurrentSpeed()              => currentSpeed;
    public bool  IsCrouching()                  => isCrouching;
    public float GetSprintSpeed()               => sprintSpeed;
    public float GetCurrentStamina()            => currentStamina;
    public float GetMaxStamina()                => maxStamina;
    public bool  CanSprint()                    => canSprint;
    public bool  HasEnoughStamina(float amount) => currentStamina >= amount;

    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
    }

    // НЕ скидає regenTimer — блок і додж не заважають відновленню після спринту
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina <= 0.01f)
        {
            currentStamina = 0f;
            canSprint = false;
        }
    }

    public void ResetSpeed() => currentSpeed = 0f;

    // ── Dodge ────────────────────────────────────────────────────────
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

    // ── Заглушки ─────────────────────────────────────────────────────
    public void TriggerExhaustion()             { }
    public void UnlockSprint()                  { }
    public void SetUnlimitedStamina(bool value) { }
}