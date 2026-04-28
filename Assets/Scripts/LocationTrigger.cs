using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Квест")]
    [SerializeField] private string questIdToComplete;
    [SerializeField] private string questIdToActivate;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (hasTriggered) return;

        // Проверяем что это игрок
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;
        TriggerQuestEvent();
    }

    private void TriggerQuestEvent()
    {
        if (QuestManager.instance == null)
        {
            Debug.LogError("❌ QuestManager не найден!");
            return;
        }

        Debug.Log($"🎯 LocationTrigger: Игрок вошел в {gameObject.name}");

        // Завершаем текущий квест
        if (!string.IsNullOrEmpty(questIdToComplete))
        {
            QuestManager.instance.CompleteQuest(questIdToComplete);
            Debug.Log($"✅ Квест '{questIdToComplete}' завершен!");
        }

        // Активируем следующий квест
        if (!string.IsNullOrEmpty(questIdToActivate))
        {
            QuestManager.instance.ActivateQuest(questIdToActivate);
            Debug.Log($"📍 Квест '{questIdToActivate}' активирован!");
        }
    }
}