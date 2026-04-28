using System.Collections;
using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    public static SleepSystem instance;
    public CanvasGroup fadeScreen; // Твоя черная шторка
    public float fadeDuration = 2f;

    private void Awake() => instance = this;

    public void StartSleeping()
    {
        StartCoroutine(SleepRoutine());
    }

    private System.Collections.IEnumerator SleepRoutine()
    {
        // 1. Включаем черную панель и затемняем экран
        if (fadeScreen == null)
        {
            Debug.LogError("❌ SleepSystem: fadeScreen не назначена в Инспекторе!");
            yield break;
        }
        
        fadeScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0, 1));

        // ✅ КВЕСТ: Выжить ночь - завершено!
        QuestManager.instance?.CompleteQuest("quest_survive_night");

        // --- ЛОГИКА СНА С ТЕНКОКУ ---
        // Ищем Тенкоку на сцене
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        
        if (tenkoku != null)
        {
            Debug.Log("🛌 Tenkoku найден! Текущее время: " + tenkoku.currentHour + ":" + tenkoku.currentMinute);
            
            float startTime = tenkoku.currentHour;
            float wakeUpTime = 8f; // Просыпаемся в 8 утра
            float hoursSlept = 0f;

            // Считаем, сколько часов Ноа проспал
            if (startTime >= 22f) 
                hoursSlept = (24f - startTime) + wakeUpTime;
            else 
                hoursSlept = wakeUpTime - startTime;

            Debug.Log($"😴 Проспали {hoursSlept} часов (эффективность сна)");

            // Считаем эффективность сна для лечения (максимум 10 часов)
            float maxSleepCycle = 10f;
            float sleepEfficiency = Mathf.Clamp01(hoursSlept / maxSleepCycle);

            // Лечим Ноа
            if (PlayerHealth.instance != null)
            {
                float missingHealth = PlayerHealth.instance.maxHealth - PlayerHealth.instance.currentHealth;
                float healthToRestore = missingHealth * sleepEfficiency;
                PlayerHealth.instance.Heal(healthToRestore);
                Debug.Log($"💚 Восстановлено {healthToRestore} HP");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerHealth.instance не найден!");
            }

            // ПЕРЕМОТКА ВРЕМЕНИ НА УТРО В ТЕНКОКУ
            tenkoku.currentHour = 8;
            tenkoku.currentMinute = 0;
            Debug.Log("⏰ Время установлено на 08:00");
        }
        else
        {
            Debug.LogError("❌ Tenkoku не найден на сцене! Система сна не работает!");
        }

        yield return new WaitForSeconds(1.5f); // Пауза в темноте для эффекта

        // 2. Осветляем экран и выключаем панель
        yield return StartCoroutine(Fade(1, 0));
        fadeScreen.gameObject.SetActive(false);

        // ✅ КВЕСТ: Найти тайник - активирован!
        QuestManager.instance?.ActivateQuest("quest_find_stash");
    }

    private IEnumerator Fade(float start, float end)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }
    }
}
