using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image healthBar;

    [Header("Lives")]
    [SerializeField] private Image[] lifeIcons;
    [SerializeField] private Image[] redLifeIcons;

    [Header("Red Border")]
    [SerializeField] private GameObject redBorderObject;
    [SerializeField] private float borderThreshold = 40f;
    [SerializeField] private float borderPulseSpeed = 3f;
    [SerializeField] private float borderMaxAlpha = 0.6f;

    [Header("Noise Effect")]
    [SerializeField] private GameObject noiseObject;
    [SerializeField] private float noiseThreshold = 20f;
    [SerializeField] private float noiseFlickerSpeed = 15f;
    [SerializeField] private float noiseMaxAlpha = 0.4f;

    private PlayerHealth playerHealth;
    private CanvasGroup borderCG;
    private CanvasGroup noiseCG;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<Image>();

        if (redBorderObject != null)
        {
            borderCG = redBorderObject.GetComponent<CanvasGroup>();
            if (borderCG == null) borderCG = redBorderObject.AddComponent<CanvasGroup>();
        }

        if (noiseObject != null)
        {
            noiseCG = noiseObject.GetComponent<CanvasGroup>();
            if (noiseCG == null) noiseCG = noiseObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        if (playerHealth == null) return;

        UpdateHealthBar();
        UpdateScreenEffects();
        UpdateLives();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;
        healthBar.fillAmount = Mathf.Clamp01(playerHealth.GetHealthPercent() / 100f);
        healthBar.color = Color.white;
    }

    private void UpdateScreenEffects()
    {
        float healthPercent = playerHealth.GetHealthPercent();

        if (borderCG != null && redBorderObject != null)
        {
            if (healthPercent <= borderThreshold)
            {
                if (!redBorderObject.activeSelf) redBorderObject.SetActive(true);
                borderCG.alpha = (Mathf.Sin(Time.time * borderPulseSpeed) * 0.5f + 0.5f) * borderMaxAlpha;
            }
            else
            {
                borderCG.alpha = 0f;
                if (redBorderObject.activeSelf) redBorderObject.SetActive(false);
            }
        }

        if (noiseCG != null && noiseObject != null)
        {
            if (healthPercent <= noiseThreshold)
            {
                if (!noiseObject.activeSelf) noiseObject.SetActive(true);
                noiseCG.alpha = (Mathf.Sin(Time.time * noiseFlickerSpeed) * 0.5f + 0.5f) * noiseMaxAlpha;
            }
            else
            {
                noiseCG.alpha = 0f;
                if (noiseObject.activeSelf) noiseObject.SetActive(false);
            }
        }
    }

    private void UpdateLives()
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;
        int currentLives = playerHealth.currentLives;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].enabled = (i < currentLives);

            if (redLifeIcons != null && i < redLifeIcons.Length && redLifeIcons[i] != null)
                redLifeIcons[i].enabled = (i >= currentLives);
        }
    }
}