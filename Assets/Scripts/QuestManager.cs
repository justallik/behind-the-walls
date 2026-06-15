using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Quests")]
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();
    private Dictionary<string, QuestData> questDict = new Dictionary<string, QuestData>();

    private QuestData currentQuest = null;

    public delegate void QuestEventHandler(QuestData quest);
    public event QuestEventHandler OnQuestActivated;
    public event QuestEventHandler OnQuestCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeQuests();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeQuests()
    {
        if (allQuests == null || allQuests.Count == 0)
        {
            QuestData[] foundQuests = Resources.LoadAll<QuestData>("Quests");
            allQuests = new List<QuestData>(foundQuests);

            if (foundQuests.Length == 0)
            {
                Debug.LogError("Квести не знайдено в Assets/Resources/Quests");
                return;
            }
        }

        foreach (QuestData quest in allQuests)
        {
            if (quest == null) continue;
            quest.Initialize();
            questDict[quest.questId] = quest;
        }

        Debug.Log($"Ініціалізовано квестів: {questDict.Count}");
    }

    // ── Геймплей — з подіями та автосейвом ──────────────────────────────────

    public void ActivateQuest(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.ActivateQuest();
        currentQuest = quest;
        OnQuestActivated?.Invoke(quest);
    }

    public void CompleteQuest(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.CompleteQuest();
        OnQuestCompleted?.Invoke(quest);

        if (!string.IsNullOrEmpty(quest.nextQuestId))
            ActivateQuest(quest.nextQuestId);

        // Автосейв після кожного виконаного квесту
        SaveSystem.instance?.Save();
    }

    public void IncrementQuestCounter(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.IncrementCounter();

        if (quest.currentCount >= quest.maxCount)
            CompleteQuest(questId); // CompleteQuest вже зберігає
        else
        {
            OnQuestActivated?.Invoke(quest);
            SaveSystem.instance?.Save(); // зберігаємо і проміжний прогрес лічильника
        }
    }

    public void TryCompleteSearchHuts()
    {
        bool hasKnife = InventorySystem.instance.HasWeapon("Knife");
        bool hasDiary = DiaryManager.instance.IsDiaryUnlocked();

        if (hasKnife && hasDiary)
            CompleteQuest("quest_search_huts");
    }

    // ── Silent — тільки для завантаження, БЕЗ подій і БЕЗ автосейву ─────────

    public void ActivateQuestSilent(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено (silent): {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.ActivateQuest();
        currentQuest = quest;
    }

    public void CompleteQuestSilent(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено (silent): {questId}");
            return;
        }

        questDict[questId].CompleteQuest();
    }

    // ── Утиліти ──────────────────────────────────────────────────────────────

    public QuestData GetQuest(string questId)
    {
        questDict.TryGetValue(questId, out QuestData q);
        return q;
    }

    public bool IsQuestActive(string questId) =>
        questDict.TryGetValue(questId, out QuestData q) && q.isActive;

    public bool IsQuestCompleted(string questId) =>
        questDict.TryGetValue(questId, out QuestData q) && q.isCompleted;

    public List<QuestData> GetAllQuests() => new List<QuestData>(questDict.Values);

    public QuestData GetCurrentQuest() => currentQuest;

    public void PrintAllQuests()
    {
        Debug.Log("=== УСІ КВЕСТИ ===");
        foreach (var quest in questDict.Values)
        {
            string status = quest.isCompleted ? "Виконано" : (quest.isActive ? "Активний" : "Неактивний");
            Debug.Log($"{status} — {quest.questId}: {quest.questObjective}");
        }
    }
}