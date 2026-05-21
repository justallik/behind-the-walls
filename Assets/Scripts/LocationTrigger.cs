using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Квест")]
    [SerializeField] private string questIdToComplete;
    [SerializeField] private string questIdToActivate;
    [SerializeField] private string questIdToIncrement;

    [Header("Проверка инвентаря")]
    [SerializeField] private bool checkInventoryOnExit = false;

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

        // Повышаем счётчик квеста
        if (!string.IsNullOrEmpty(questIdToIncrement))
        {
            QuestManager.instance.IncrementQuestCounter(questIdToIncrement);
            Debug.Log($"📍 Счётчик квеста '{questIdToIncrement}' повышен!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!checkInventoryOnExit) return;
        if (!other.CompareTag("Player")) return;

        bool hasKnife = InventorySystemNew.instance.HasWeapon("Knife");
        bool hasDiary = DiaryManager.instance.IsDiaryUnlocked();

        if (hasKnife && hasDiary)
        {
            Debug.Log("🎬 Выход — запускаем катсцену!");
            QuestManager.instance.CompleteQuest("quest_leave_hut");
            QuestManager.instance.ActivateQuest("quest_survive");
        }
        else
        {
            Debug.Log("⚠️ Нет ножа или дневника!");
        }
    }
}