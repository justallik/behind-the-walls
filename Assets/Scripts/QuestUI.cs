using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class QuestUI : MonoBehaviour
{
    [Header("Анимация")]
    [SerializeField] private Animator animator;

    [Header("Контент")]
    [SerializeField] private TextMeshProUGUI questText;
    [SerializeField] private GameObject questPanel;

    private QuestManager questManager;
    private bool isOpen = false;
    private bool hasQuest = false;

    private void Start()
    {
        questManager = QuestManager.instance;

        if (questManager == null)
        {
            Debug.LogError("❌ QuestManager не найден!");
            return;
        }

        questManager.OnQuestActivated += OnQuestActivated;
        questManager.OnQuestCompleted += OnQuestCompleted;

        if (questPanel != null)
            questPanel.SetActive(false);
    }

    private void Update()
    {
        if (hasQuest && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isOpen) Close();
            else Open();
        }
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestActivated -= OnQuestActivated;
            questManager.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    private void OnQuestActivated(QuestData quest)
    {
        hasQuest = true;

        if (questPanel != null)
            questPanel.SetActive(true);

        if (questText != null)
            questText.text = quest.GetFullObjective();

        Open();
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (questText != null)
            questText.text = $"✅ {quest.GetFullObjective()}";
    }

    private void Open()
    {
        isOpen = true;
        if (animator != null)
            animator.Play("QuestOpen");
    }

    private void Close()
    {
        isOpen = false;
        if (animator != null)
            animator.Play("QuestClose");
    }
}


