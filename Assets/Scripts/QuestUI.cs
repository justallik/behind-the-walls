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

        // Скасовуємо закриття якщо воно було заплановане
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (questPanel != null)
            questPanel.SetActive(true);

        if (questText != null)
            questText.text = quest.GetFullObjective();

        // Відкриваємо тільки якщо панель закрита
        if (!isOpen)
            Open();
    }

    private void OnQuestCompleted(QuestData quest)
    {
        hasQuest = false;

        // Закриваємо з затримкою — якщо прийде новий квест то закриття скасується
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