using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("UI - Простой текст задания")]
    [SerializeField] private TextMeshProUGUI questText; // Одна строка!
    [SerializeField] private GameObject questPanel;

    [Header("Квесты")]
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();
    private Dictionary<string, QuestData> questDict = new Dictionary<string, QuestData>();
    
    private QuestData currentQuest = null;
    
    // ==================== СОБЫТИЯ ====================
    public delegate void QuestEventHandler(QuestData quest);
    public event QuestEventHandler OnQuestActivated;
    public event QuestEventHandler OnQuestCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("🚀 QuestManager Start()");
        InitializeQuests();
    }

    // ==================== ИНИЦИАЛИЗАЦИЯ ====================
    private void InitializeQuests()
    {
        Debug.Log("📍 InitializeQuests() запущена");
        
        // Загружаем все QuestData из массива
        if (allQuests == null || allQuests.Count == 0)
        {
            QuestData[] foundQuests = Resources.LoadAll<QuestData>("Quests");
            allQuests = new List<QuestData>(foundQuests);
            Debug.Log($"📋 Загружено {allQuests.Count} квестов из Resources/Quests");
            
            if (foundQuests.Length == 0)
            {
                Debug.LogError("❌ НЕТУ КВЕСТОВ в Assets/Resources/Quests!");
                return;
            }
        }

        foreach (QuestData quest in allQuests)
        {
            if (quest == null) continue;
            quest.Initialize();
            questDict[quest.questId] = quest;
            Debug.Log($"✅ Инициализирован квест: {quest.questId}");
        }

        Debug.Log($"📊 Всего инициализировано квестов: {questDict.Count}");

        // Скрываем панель в начале
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    // ==================== АКТИВАЦИЯ КВЕСТА ====================
    public void ActivateQuest(string questId)
    {
        Debug.Log($"🎯 ActivateQuest('{questId}') вызвана");
        
        if (questDict == null || questDict.Count == 0)
        {
            Debug.LogError("❌ questDict пусто! Инициализация не прошла");
            return;
        }

        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"❌ Квест не найден в словаре: {questId}");
            Debug.Log($"📋 Доступные квесты: {string.Join(", ", questDict.Keys)}");
            return;
        }

        QuestData quest = questDict[questId];
        if (quest == null)
        {
            Debug.LogError($"❌ QuestData is null для: {questId}");
            return;
        }

        Debug.Log($"✅ Найден квест: {quest.questObjective}");
        quest.ActivateQuest();
        currentQuest = quest;
        
        UpdateQuestUI();
        OnQuestActivated?.Invoke(quest);
        Debug.Log($"✅ Квест активирован!");
    }

    // ==================== ЗАВЕРШЕНИЕ КВЕСТА ====================
    public void CompleteQuest(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"❌ Квест не найден: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.CompleteQuest();
        UpdateQuestUI();
        OnQuestCompleted?.Invoke(quest);
    }

    // ==================== УВЕЛИЧЕНИЕ СЧЁТЧИКА ====================
    public void IncrementQuestCounter(string questId)
    {
        if (!questDict.ContainsKey(questId))
        {
            Debug.LogError($"❌ Квест не найден: {questId}");
            return;
        }

        QuestData quest = questDict[questId];
        quest.IncrementCounter();
        UpdateQuestUI();

        if (quest.isCompleted)
        {
            OnQuestCompleted?.Invoke(quest);
        }
    }

    // ==================== ОБНОВЛЕНИЕ UI ====================
    private void UpdateQuestUI()
    {
        Debug.Log("🖼️ UpdateQuestUI() вызвана");
        
        if (questText == null)
        {
            Debug.LogError("❌ questText НЕ НАЗНАЧЕН в Inspector!");
            return;
        }
        if (questPanel == null)
        {
            Debug.LogWarning("⚠️ questPanel НЕ назначен, но это не критично");
        }

        if (currentQuest == null || !currentQuest.isActive)
        {
            Debug.Log("📍 Квест не активен, скрываем UI");
            if (questPanel != null)
                questPanel.SetActive(false);
            questText.text = "";
            return;
        }

        Debug.Log("✅ Показываем UI квеста");
        if (questPanel != null)
            questPanel.SetActive(true);

        questText.text = $"📍 {currentQuest.GetFullObjective()}";
        Debug.Log($"💬 Текст установлен: {questText.text}");
    }

    // ==================== ПОЛУЧЕНИЕ ИНФОРМАЦИИ ====================
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

    // ==================== СОВМЕСТИМОСТЬ СО СТАРЫМ КОДОМ ====================
    public void UpdateQuest(string questText)
    {
        Debug.Log($"📋 {questText}");
    }

    // ==================== DEBUG ====================
    public void PrintAllQuests()
    {
        Debug.Log("=== ВСЕ КВЕСТЫ ===");
        foreach (var quest in questDict.Values)
        {
            string status = quest.isCompleted ? "✅" : (quest.isActive ? "🟡" : "⚪");
            Debug.Log($"{status} {quest.questId}: {quest.questObjective}");
        }
    }
}

