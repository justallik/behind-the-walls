using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Image staminaImage;

    [Header("Settings")]
    [SerializeField] private float fillAnimationSpeed = 5f;

    private float targetFillAmount = 1f;
    private float lastStamina = -1f;

    private void Update()
    {
        if (playerMovement != null && staminaImage != null)
        {
            float currentStamina = playerMovement.GetCurrentStamina();
            if (currentStamina != lastStamina)
            {
                targetFillAmount = currentStamina / playerMovement.GetMaxStamina();
                lastStamina = currentStamina;
            }

            staminaImage.fillAmount = Mathf.Lerp(
                staminaImage.fillAmount,
                targetFillAmount,
                Time.deltaTime * fillAnimationSpeed
            );
        }
    }
}