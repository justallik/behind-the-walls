using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class QuestUI : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Content")]
    [SerializeField] private TextMeshProUGUI questText;
    [SerializeField] private GameObject questPanel;

    [Header("Settings")]
    [SerializeField] private float closeDelay = 1.5f;

    private QuestManager questManager;
    private bool isOpen = false;
    private bool hasQuest = false;
    private Coroutine closeCoroutine;

    private void Start()
    {
        questManager = QuestManager.instance;

        if (questManager == null)
        {
            Debug.LogError("QuestManager не знайдено");
            return;
        }

        questManager.OnQuestActivated += OnQuestActivated;
        questManager.OnQuestCompleted += OnQuestCompleted;

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

    // Викликається з SaveSystem після завершення Load() —
    // гарантовано після того як квести завантажені І HUDRoot активний
    public void RefreshAfterLoad()
    {
        if (questManager == null) return;

        // Беремо поточний квест — останній активований при завантаженні
        QuestData current = questManager.GetCurrentQuest();
        if (current != null && current.isActive && !current.isCompleted)
        {
            hasQuest = true;
            if (questPanel != null) questPanel.SetActive(true);
            if (questText != null) questText.text = current.GetFullObjective();
            isOpen = true;
            return;
        }

        // Fallback — якщо currentQuest чомусь null, шукаємо будь-який активний
        foreach (QuestData quest in questManager.GetAllQuests())
        {
            if (quest.isActive && !quest.isCompleted)
            {
                hasQuest = true;
                if (questPanel != null) questPanel.SetActive(true);
                if (questText != null) questText.text = quest.GetFullObjective();
                isOpen = true;
                return;
            }
        }
    }

    private void Update()
    {
        if (hasQuest && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isOpen) Close();
            else Open();
        }
    }

    private void OnQuestActivated(QuestData quest)
    {
        hasQuest = true;

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (questPanel != null)
            questPanel.SetActive(true);

        if (questText != null)
            questText.text = quest.GetFullObjective();

        if (!isOpen)
            Open();
    }

    private void OnQuestCompleted(QuestData quest)
    {
        hasQuest = false;

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        if (!hasQuest)
        {
            if (questText != null)
                questText.text = "";

            isOpen = false;

            if (questPanel != null)
                questPanel.SetActive(false);
        }

        closeCoroutine = null;
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