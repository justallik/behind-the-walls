using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier = 1f;

    private bool isDead = false;
    private bool isLying = true;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        if (playerCombat == null)
            playerCombat = GetComponentInParent<PlayerCombat>();

        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        if (animator != null)
            animator.SetBool("IsLying", true);
    }

    private void Update()
    {
        if (animator == null || isDead) return;

        if (playerHealth != null && playerHealth.currentHealth <= 0 && !isDead)
        {
            isDead = true;
            animator.SetTrigger("Die");
            return;
        }

        if (isLying)
        {
            bool lying = animator.GetBool("IsLying");
            if (!lying) isLying = false;
            return;
        }

        float speed = playerMovement != null ? playerMovement.GetCurrentSpeed() : 0f;
        float normalized = Mathf.Clamp01(speed / (playerMovement != null ? playerMovement.GetSprintSpeed() : 8f));
        animator.SetFloat("Speed", normalized * speedMultiplier, 0.1f, Time.deltaTime);

        bool blocking = playerCombat != null && playerCombat.isBlocking;
        animator.SetBool("Block", blocking);
    }

    private void OnAnimatorMove()
    {
        // Root motion заблокований — CharacterController керує рухом
    }

    public void StandUp()
    {
        isLying = false;
        if (animator != null)
            animator.SetBool("IsLying", false);
    }

    public void TriggerAttack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public void TriggerDodge()
    {
        if (animator != null)
            animator.SetTrigger("Dodge");
    }
}