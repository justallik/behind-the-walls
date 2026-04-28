using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("UI - Одна строка задания")]
    [SerializeField] private TextMeshProUGUI questText;
    [SerializeField] private GameObject questPanel;

    private QuestManager questManager;

    private void Start()
    {
        questManager = QuestManager.instance;
        
        if (questManager == null)
        {
            Debug.LogError("❌ QuestManager не найден!");
            return;
        }

        // Подписываемся на события
        questManager.OnQuestActivated += OnQuestActivated;
        questManager.OnQuestCompleted += OnQuestCompleted;

        // Скрываем панель в начале
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestActivated -= OnQuestActivated;
            questManager.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    // ==================== СОБЫТИЯ ====================
    private void OnQuestActivated(QuestData quest)
    {
        DisplayQuest(quest);
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (questText != null)
            questText.text = $"✅ {quest.GetFullObjective()}";
    }

    // ==================== ОТОБРАЖЕНИЕ ====================
    private void DisplayQuest(QuestData quest)
    {
        if (quest == null) return;

        if (questPanel != null) questPanel.SetActive(true);
        if (questText != null) questText.text = $"📍 {quest.GetFullObjective()}";
    }
}


