using System.Collections;
using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    public static SleepSystem instance;

    [Header("UI")]
    public CanvasGroup fadeScreen;
    public float fadeDuration = 2f;

    private void Awake() => instance = this;

    public void StartSleeping()
    {
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        if (fadeScreen == null)
        {
            Debug.LogError("❌ SleepSystem: fadeScreen не призначена в Інспекторі!");
            yield break;
        }

        // 1. Затемнюємо екран
        fadeScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0f, 1f));

        // ✅ КВЕСТ: Вижити ніч
        QuestManager.instance?.CompleteQuest("quest_survive_night");

        // --- ТЕНКОКУ ---
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku != null)
        {
            float startTime = tenkoku.currentHour;
            float wakeUpTime = 8f;
            float hoursSlept = startTime >= 22f
                ? (24f - startTime) + wakeUpTime
                : wakeUpTime - startTime;

            float sleepEfficiency = Mathf.Clamp01(hoursSlept / 10f);

            if (PlayerHealth.instance != null)
            {
                float missing  = PlayerHealth.instance.maxHealth - PlayerHealth.instance.currentHealth;
                float restored = missing * sleepEfficiency;
                PlayerHealth.instance.Heal(restored);
                Debug.Log($"💚 Відновлено {restored} HP");
            }

            tenkoku.currentHour   = 8;
            tenkoku.currentMinute = 0;
            Debug.Log("⏰ Час встановлено 08:00");
        }
        else
        {
            Debug.LogWarning("⚠️ Tenkoku не знайдено");
        }

        // 2. Пауза в темряві — камера непомітно повертається вгору
        yield return new WaitForSeconds(1.5f);

        // 3. Запускаємо фінальну сцену (екран ще чорний)
        if (EndingController.instance != null)
        {
            EndingController.instance.StartEnding();
        }
        else
        {
            Debug.LogError("❌ EndingController не знайдено на сцені!");
        }
    }

    public void HideFade()
    {
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0f;
            fadeScreen.gameObject.SetActive(false);
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = end;
    }
}