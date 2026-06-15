using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Save ID")]
    [SerializeField] private string uniqueId; // задати в Inspector — унікальний для кожного тригера

    [Header("Quest Requirement")]
    [SerializeField] private string requiredQuestCompleted;
    [SerializeField] private string requiredQuestActive;

    [Header("Dialogue")]
    [SerializeField] private string line;
    [SerializeField] private float hideAfterSeconds = 4f;
    [SerializeField] private bool triggerOnExit = false;

    [Header("Voice")]
    [SerializeField] private AudioClip voiceClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Invisible Wall")]
    [SerializeField] private GameObject invisibleWall;

    [Header("Quest")]
    [SerializeField] private bool completeQuest;
    [SerializeField] private string completeQuestId;
    [SerializeField] private bool activateQuest;
    [SerializeField] private string activateQuestId;

    private bool _triggered;
    private bool _ready = false;

    private void Start()
    {
        if (invisibleWall != null)
            invisibleWall.SetActive(true);

        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted += OnQuestCompleted;

        Invoke(nameof(SetReady), 0.5f);
    }

    private void SetReady() => _ready = true;

    // Викликається SaveSystem.Load()
    public void OnLoadSave()
    {
        if (string.IsNullOrEmpty(uniqueId)) return;
        if (!SaveSystem.instance.IsTriggered(uniqueId)) return;

        // Тригер вже спрацював — відключаємо діалог назавжди
        _triggered = true;

        // Стіну прибираємо якщо квест вже виконаний
        if (invisibleWall != null && !string.IsNullOrEmpty(requiredQuestActive))
        {
            if (QuestManager.instance != null &&
                QuestManager.instance.IsQuestCompleted(requiredQuestActive))
            {
                invisibleWall.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
            QuestManager.instance.OnQuestCompleted -= OnQuestCompleted;
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (quest.questId == requiredQuestActive && invisibleWall != null)
            invisibleWall.SetActive(false);
    }

    private bool IsRequiredQuestDone()
    {
        if (string.IsNullOrEmpty(requiredQuestCompleted)) return true;
        if (QuestManager.instance == null) return false;
        return QuestManager.instance.IsQuestCompleted(requiredQuestCompleted);
    }

    private bool IsRequiredQuestActive()
    {
        if (string.IsNullOrEmpty(requiredQuestActive)) return true;
        if (QuestManager.instance == null) return false;
        return !QuestManager.instance.IsQuestCompleted(requiredQuestActive);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnExit) return;
        HandleTrigger(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_ready) return;
        if (!triggerOnExit) return;
        HandleTrigger(other);
    }

    private void HandleTrigger(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;
        if (!IsRequiredQuestDone()) return;
        if (!IsRequiredQuestActive()) return;

        _triggered = true;

        if (!string.IsNullOrEmpty(uniqueId))
            SaveSystem.instance?.RegisterTriggered(uniqueId);

        if (!string.IsNullOrEmpty(line))
        {
            DialogueManager.Instance.ShowLine(line);

            if (voiceClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(voiceClip);
                Invoke(nameof(Hide), voiceClip.length + 0.3f);
            }
            else
            {
                Invoke(nameof(Hide), hideAfterSeconds);
            }
        }

        if (completeQuest && !string.IsNullOrEmpty(completeQuestId))
            QuestManager.instance.CompleteQuest(completeQuestId);

        if (activateQuest && !string.IsNullOrEmpty(activateQuestId))
            QuestManager.instance.ActivateQuest(activateQuestId);
    }

    private void Hide() => DialogueManager.Instance.HideDialogue();
}