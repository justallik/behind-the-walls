using UnityEngine;

public class TimeProgression : MonoBehaviour
{
    [System.Serializable]
    public class QuestTimeEntry
    {
        public string questId;
        public float targetHour; // час на который переключить время
    }

    [Header("Привязка времени к квестам")]
    public QuestTimeEntry[] questTimeSteps;

    [Header("Плавность перехода")]
    public float transitionSpeed = 10f; // насколько быстро время "догоняет" цель

    private Tenkoku.Core.TenkokuModule tenkoku;
    private float targetTime = -1f;

    private void Start()
    {
        tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        if (tenkoku == null)
        {
            Debug.LogError("❌ TimeProgression: Tenkoku не найден!");
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

        // Плавно двигаем время к цели
        float current = tenkoku.currentHour;
        if (Mathf.Abs(current - targetTime) > 0.05f)
            tenkoku.currentHour = Mathf.MoveTowards(current, targetTime, transitionSpeed * Time.deltaTime);
        else
            targetTime = -1f; // достигли цели
    }

    private void OnQuestCompleted(QuestData quest)
    {
        foreach (var entry in questTimeSteps)
        {
            if (entry.questId == quest.questId)
            {
                targetTime = entry.targetHour;
                Debug.Log($"⏰ Квест '{quest.questId}' завершён → время движется к {entry.targetHour}:00");
                return;
            }
        }
    }
}
