using UnityEngine;

public class InteractableBed : MonoBehaviour
{
    public void Interact()
    {
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku == null)
        {
            Debug.LogError("Tenkoku не найден");
            return;
        }

        float time = tenkoku.currentHour;
        bool canSleep = (time >= 18f || time < 8f);

        if (!canSleep)
        {
            Debug.Log($"Ноа не хочет спать в {time:F1}:00. Можно спать только с 22:00 до 08:00");
            return;
        }

        if (SleepSystem.instance == null)
        {
            Debug.LogError("SleepSystem не найден");
            return;
        }

        SleepSystem.instance.StartSleeping();
    }
}