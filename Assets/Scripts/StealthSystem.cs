using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    public static StealthSystem instance;

    private bool isStealth = false;

    [Header("Settings")]
    [SerializeField] private float stealthTransparency = 0.3f;

    private Renderer playerRenderer;
    private Color originalColor;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
    }

    public void EnableStealth()
    {
        if (isStealth) return;
        isStealth = true;

        if (playerRenderer != null)
        {
            Color stealthColor = originalColor;
            stealthColor.a = stealthTransparency;
            playerRenderer.material.color = stealthColor;
        }
    }

    public void DisableStealth()
    {
        if (!isStealth) return;
        isStealth = false;

        if (playerRenderer != null)
            playerRenderer.material.color = originalColor;
    }

    public bool IsStealth() => isStealth;

    public void BreakStealth()
    {
        if (isStealth) DisableStealth();
    }
}