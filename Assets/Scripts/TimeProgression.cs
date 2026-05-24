using UnityEngine;

public class TimeProgression : MonoBehaviour
{
    [System.Serializable]
    public class QuestTimeEntry
    {
        public string questId;
        public float targetHour;
    }

    [Header("Quest Time Steps")]
    public QuestTimeEntry[] questTimeSteps;

    [Header("Settings")]
    public float transitionSpeed = 10f;

    private Tenkoku.Core.TenkokuModule tenkoku;
    private float targetTime = -1f;

    private void Start()
    {
        tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku == null)
        {
            Debug.LogError("TimeProgression: Tenkoku не знайдено");
            return;
        }

        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted -= OnQuestCompleted;
    }

    private void Update()
    {
        if (tenkoku == null || targetTime < 0f) return;

        float current = tenkoku.currentHour;
        if (Mathf.Abs(current - targetTime) > 0.05f)
            tenkoku.currentHour = (int)Mathf.MoveTowards(current, targetTime, transitionSpeed * Time.deltaTime);
        else
            targetTime = -1f;
    }

    private void OnQuestCompleted(QuestData quest)
    {
        foreach (var entry in questTimeSteps)
        {
            if (entry.questId == quest.questId)
            {
                targetTime = entry.targetHour;
                Debug.Log($"Квест '{quest.questId}' завершено — час рухається до {entry.targetHour}:00");
                return;
            }
        }
    }
}