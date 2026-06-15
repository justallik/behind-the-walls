using System.Collections;
using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    public static SleepSystem instance;
    public CanvasGroup fadeScreen;
    public float fadeDuration = 2f;

    private void Awake() { instance = this; }

    public void StartSleeping() { StartCoroutine(SleepRoutine()); }

    private IEnumerator SleepRoutine()
    {
        if (fadeScreen == null) yield break;
        fadeScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0f, 1f));
        QuestManager.instance?.CompleteQuest("quest_survive_night");
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku != null)
        {
            if (PlayerHealth.instance != null) PlayerHealth.instance.Heal(PlayerHealth.instance.maxHealth);
            tenkoku.currentHour = 8;
            tenkoku.currentMinute = 0;
        }
        yield return new WaitForSeconds(1.5f);
        EndingController.instance?.StartEnding();
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
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = end;
    }
}