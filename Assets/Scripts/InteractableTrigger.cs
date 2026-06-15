using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableTrigger : MonoBehaviour
{
    [Header("Save ID")]
    [SerializeField] private string uniqueId; // задати в Inspector — унікальний для кожного тригера

    [Header("Quest Requirement")]
    [SerializeField] private string requiredQuestCompleted;

    [Header("Quest")]
    [SerializeField] private string questIdToIncrement;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private void Start()
    {
        // Start не використовуємо для ініціалізації — OnLoadSave викликається з SaveSystem
    }

    // Викликається SaveSystem.Load()
    public void OnLoadSave()
    {
        if (string.IsNullOrEmpty(uniqueId)) return;
        if (!SaveSystem.instance.IsTriggered(uniqueId)) return;

        hasInteracted = true; // вже взаємодіяли — більше не реагує
    }

    private bool IsRequiredQuestDone()
    {
        if (string.IsNullOrEmpty(requiredQuestCompleted)) return true;
        if (QuestManager.instance == null) return false;
        return QuestManager.instance.IsQuestCompleted(requiredQuestCompleted);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && !hasInteracted &&
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!IsRequiredQuestDone()) return;
            Interact();
        }
    }

    private void Interact()
    {
        hasInteracted = true;

        if (!string.IsNullOrEmpty(uniqueId))
            SaveSystem.instance?.RegisterTriggered(uniqueId);

        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager не знайдено");
            return;
        }

        QuestManager.instance.IncrementQuestCounter(questIdToIncrement);
    }
}