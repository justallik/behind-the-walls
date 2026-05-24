using UnityEngine;
using TMPro;
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

    public void ActivateQuest(string questId)
    {
        if (questDict == null || questDict.Count == 0)
        {
            Debug.LogError("questDict порожній");
            return;
        }

        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"Квест не знайдено: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        if (quest == null)
        {
            Debug.LogError($"QuestData є null для: {questId}");
            return;
        }

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
            CompleteQuest(questId);
        else
            OnQuestActivated?.Invoke(quest);
    }

    public void TryCompleteSearchHuts()
    {
        bool hasKnife = InventorySystem.instance.HasWeapon("Knife");
        bool hasDiary = DiaryManager.instance.IsDiaryUnlocked();

        if (hasKnife && hasDiary)
            CompleteQuest("quest_search_huts");
    }

    public QuestData GetQuest(string questId)
    {
        if (questDict.ContainsKey(questId))
            return questDict[questId];
        return null;
    }

    public bool IsQuestActive(string questId)
    {
        if (questDict.ContainsKey(questId))
            return questDict[questId].isActive;
        return false;
    }

    public bool IsQuestCompleted(string questId)
    {
        if (questDict.ContainsKey(questId))
            return questDict[questId].isCompleted;
        return false;
    }

    public List<QuestData> GetAllQuests() => new List<QuestData>(questDict.Values);

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