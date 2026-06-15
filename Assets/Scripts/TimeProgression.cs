// using UnityEngine;

// public class TimeProgression : MonoBehaviour
// {
//     [System.Serializable]
//     public class QuestTimeEntry
//     {
//         public string questId;
//         public float targetHour;
//     }

//     [Header("Quest Time Steps")]
//     public QuestTimeEntry[] questTimeSteps;

//     [Header("Settings")]
//     public float transitionDuration = 12f; 

//     private Tenkoku.Core.TenkokuModule tenkoku;
//     private float internalCurrentTime = -1f; 
//     private float targetTime = -1f;
//     private float currentTransitionSpeed = 0f;

//     private void Start()
//     {
//         tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
//         if (tenkoku == null)
//         {
//             Debug.LogError("TimeProgression: Tenkoku not found");
//             return;
//         }

//         if (QuestManager.instance != null)
//             QuestManager.instance.OnQuestCompleted += OnQuestCompleted;
//     }

//     private void OnDestroy()
//     {
//         if (QuestManager.instance != null)
//             QuestManager.instance.OnQuestCompleted -= OnQuestCompleted;
//     }

//     private void Update()
//     {
//         if (tenkoku == null || targetTime < 0f) return;

//         if (internalCurrentTime < 0f)
//         {
//             internalCurrentTime = tenkoku.currentHour;
//         }

//         if (Mathf.Abs(internalCurrentTime - targetTime) > 0.01f)
//         {
//             internalCurrentTime = Mathf.MoveTowards(internalCurrentTime, targetTime, currentTransitionSpeed * Time.deltaTime);
            
//             int hourToSet = Mathf.FloorToInt(internalCurrentTime);
//             if (hourToSet >= 24) hourToSet -= 24; 
            
//             tenkoku.currentHour = hourToSet;
//         }
//         else
//         {
//             int finalHour = Mathf.FloorToInt(targetTime);
//             if (finalHour >= 24) finalHour -= 24;

//             tenkoku.currentHour = finalHour; 
//             targetTime = -1f;
//             internalCurrentTime = -1f;
//         }
//     }

//     private void OnQuestCompleted(QuestData quest)
//     {
//         foreach (var entry in questTimeSteps)
//         {
//             if (entry.questId == quest.questId)
//             {
//                 internalCurrentTime = tenkoku.currentHour;
//                 float currentHour = internalCurrentTime;
//                 float targetHour = entry.targetHour;

//                 if (targetHour < currentHour)
//                 {
//                     targetHour += 24f; 
//                 }

//                 targetTime = targetHour;

//                 float distance = Mathf.Abs(targetTime - currentHour);
//                 currentTransitionSpeed = distance / transitionDuration;

//                 Debug.Log("Quest " + quest.questId + " completed. Transitioning to " + entry.targetHour + ".");
//                 return;
//             }
//         }
//     }
// }


using System.Collections;
using UnityEngine;

public class TimeProgression : MonoBehaviour
{
    [System.Serializable]
    public class QuestTimeEntry
    {
        public string questId;
        // Target time in 24h format. Fraction = fraction of an hour
        // (18.5 = 18:30). To force "next morning" past midnight you can also
        // use values above 24 here (e.g. 32 = 08:00 next day), but normally
        // just set 8 and the script wraps forward automatically.
        public float targetHour;
    }

    [Header("Quest Time Steps")]
    public QuestTimeEntry[] questTimeSteps;

    [Header("Settings")]
    // How long (in real seconds) the on-screen transition takes.
    public float transitionDuration = 4f;

    private Tenkoku.Core.TenkokuModule tenkoku;
    private float storyTime;        // current story time in hours [0..24)
    private Coroutine activeRoutine;

    private void Start()
    {
        tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku == null)
        {
            Debug.LogError("TimeProgression: Tenkoku not found");
            return;
        }

        // Start from whatever time the scene currently shows.
        storyTime = tenkoku.currentHour + (tenkoku.currentMinute / 60f);

        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted -= OnQuestCompleted;
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (tenkoku == null) return;

        foreach (var entry in questTimeSteps)
        {
            if (entry.questId == quest.questId)
            {
                float from = storyTime;
                float to = entry.targetHour;

                // Always move FORWARD by the intended gap only. If the target
                // looks "behind" us, it means the next day (e.g. 20:00 -> 8:00).
                while (to < from - 0.0001f) to += 24f;

                if (activeRoutine != null) StopCoroutine(activeRoutine);
                activeRoutine = StartCoroutine(MoveTime(from, to));

                Debug.Log("Quest " + quest.questId + " completed. Time -> " + Format(to) + ".");
                return;
            }
        }
    }

    private IEnumerator MoveTime(float from, float to)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.0001f, transitionDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyTime(Mathf.Lerp(from, to, t));
            yield return null;
        }

        ApplyTime(to);
        storyTime = Mathf.Repeat(to, 24f); // keep normalized for the next step
        activeRoutine = null;
    }

    private void ApplyTime(float hour)
    {
        hour = Mathf.Repeat(hour, 24f);

        int h = Mathf.FloorToInt(hour);
        float minF = (hour - h) * 60f;
        int m = Mathf.FloorToInt(minF);
        int s = Mathf.FloorToInt((minF - m) * 60f);
        if (m > 59) m = 59;
        if (s > 59) s = 59;

        tenkoku.currentHour = h;
        tenkoku.currentMinute = m;
        tenkoku.currentSecond = s;
    }

    private string Format(float hour)
    {
        hour = Mathf.Repeat(hour, 24f);
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60f);
        return h.ToString("00") + ":" + m.ToString("00");
    }
}



