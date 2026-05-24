using UnityEngine;
using UnityEngine.InputSystem;

public class StealthToggle : MonoBehaviour
{
    private StealthSystem stealthSystem;

    private void Start()
    {
        stealthSystem = GetComponent<StealthSystem>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
            ToggleStealth();
    }

    public void ToggleStealth()
    {
        if (stealthSystem == null) return;

        if (stealthSystem.IsStealth())
            stealthSystem.DisableStealth();
        else
            stealthSystem.EnableStealth();
    }

    public void ActivateStealthItem()
    {
        if (stealthSystem != null)
            stealthSystem.EnableStealth();
    }
}