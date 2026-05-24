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
    [SerializeField] private AnimationCurve staminaDrainCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);
    private float regenTimer = 0f;

    [Header("Dodge")]
    [SerializeField] private float dodgeForce = 12f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeRecoveryTime = 0.15f;

    private CharacterController controller;
    private PlayerInputHandler inputHandler;

    private float currentSpeed = 0f;
    private Vector3 velocity = Vector3.zero;
    private float currentStamina;

    private bool isGrounded;
    private bool isCrouching;
    private bool canSprint = true;
    private bool isDodging = false;
    private bool unlimitedStamina = false;

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
        UpdateStamina();
        ApplyGravity();
        HandleCrouch();
        Move();
    }

    private void GroundedCheck()
    {
        cachedSpherePosition.x = transform.position.x;
        cachedSpherePosition.y = transform.position.y - groundedOffset;
        cachedSpherePosition.z = transform.position.z;
        isGrounded = Physics.CheckSphere(cachedSpherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void UpdateStamina()
    {
        if (unlimitedStamina)
        {
            currentStamina = maxStamina;
            canSprint = true;
            return;
        }

        bool isMoving = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ;
        bool isSprinting = inputHandler.SprintInput && !isCrouching && currentSpeed > 0;

        if (regenTimer > 0) regenTimer -= Time.deltaTime;

        if (isSprinting && currentStamina > 0)
        {
            float normalizedStamina = currentStamina / maxStamina;
            float curveMultiplier = staminaDrainCurve.Evaluate(normalizedStamina);
            currentStamina -= staminaDrainRate * curveMultiplier * Time.deltaTime;
            regenTimer = regenDelay;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
                Debug.Log("Витривалість вичерпана");
            }
        }
        else if (currentStamina < maxStamina && regenTimer <= 0)
        {
            if (!isMoving)
                currentStamina += staminaRecoveryIdle * Time.deltaTime;
            else if (isMoving && !isSprinting)
                currentStamina += staminaRecoveryWalk * Time.deltaTime;

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
        if (wantCrouch != isCrouching)
        {
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
    }

    private void Move()
    {
        float targetSpeed = inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ
            ? (inputHandler.SprintInput && !isCrouching && canSprint ? sprintSpeed : normalSpeed)
            : 0f;

        if (isCrouching && targetSpeed > 0) targetSpeed = crouchSpeed;

        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f)
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        else
            currentSpeed = targetSpeed;

        Vector3 inputDirection = Vector3.zero;
        if (inputHandler.MoveInput.sqrMagnitude > MOVE_INPUT_THRESHOLD_SQ)
            inputDirection = (transform.right * inputHandler.MoveInput.x + transform.forward * inputHandler.MoveInput.y).normalized;

        Vector3 movement = inputDirection * currentSpeed * Time.deltaTime + new Vector3(0, velocity.y * Time.deltaTime, 0);
        controller.Move(movement);
    }

    public float GetCurrentSpeed() => currentSpeed;
    public bool IsCrouching() => isCrouching;
    public float GetSprintSpeed() => sprintSpeed;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public bool CanSprint() => canSprint;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public void UseStamina(float amount)
    {
        if (unlimitedStamina) return;
        currentStamina -= amount;
        regenTimer = regenDelay;
        if (currentStamina <= 0)
        {
            currentStamina = 0f;
            canSprint = false;
        }
    }

    public void TriggerExhaustion()
    {
        if (unlimitedStamina) return;
        canSprint = false;
        regenTimer = regenDelay;
        Debug.Log("Ноа знесилений — спринт заблоковано");
    }

    public void SetUnlimitedStamina(bool value)
    {
        unlimitedStamina = value;
        if (value) currentStamina = maxStamina;
    }

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

    public void UnlockSprint() => canSprint = true;

    public void ResetSpeed()
    {
        currentSpeed = 0f;
    }
}