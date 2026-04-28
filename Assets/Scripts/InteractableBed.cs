using UnityEngine;

public class InteractableBed : MonoBehaviour
{
    public void Interact()
    {
        // 1. Ищем скрипт Тенкоку на нашей сцене
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        
        if (tenkoku == null)
        {
            Debug.LogError("❌ Tenkoku не найден! Невозможно узнать время.");
            return;
        }

        // 2. Спрашиваем у него текущий час
        float time = tenkoku.currentHour;
        Debug.Log($"🛏️ Попытка спать. Текущее время: {time:F1}:00");

        // 3. Проверка: можно спать только с 22:00 вечера до 08:00 утра
        bool canSleep = (time >= 22f || time < 8f);

        if (!canSleep)
        {
            Debug.Log($"☀️ Ноа не хочет спать в {time:F1}:00. Можно спать только с 22:00 до 08:00.");
            return;
        }

        // 4. Если всё ок - запускаем сон!
        Debug.Log("🛏️ Ноа ложится спать...");
        
        if (SleepSystem.instance == null)
        {
            Debug.LogError("❌ SleepSystem.instance не найден!");
            return;
        }
        
        SleepSystem.instance.StartSleeping();
    }
}
