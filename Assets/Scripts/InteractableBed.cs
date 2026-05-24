using UnityEngine;

public class InteractableBed : MonoBehaviour
{
    public void Interact()
    {
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        
        if (tenkoku == null)
        {
            Debug.LogError("Tenkoku не знайдено");
            return;
        }

        float time = tenkoku.currentHour;
        bool canSleep = (time >= 22f || time < 8f);

        if (!canSleep)
        {
            Debug.Log($"Ноа не хоче спати о {time:F1}:00. Можна спати лише з 22:00 до 08:00");
            return;
        }

        if (SleepSystem.instance == null)
        {
            Debug.LogError("SleepSystem не знайдено");
            return;
        }

        SleepSystem.instance.StartSleeping();
    }
}