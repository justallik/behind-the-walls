using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public ItemData itemData;

    [Header("Квест")]
    [TextArea]
    public string questTextOnPickup;

    private void Start()
    {
        // Скрываем записки если дневник не разблокирован
        if (itemData == null) return;
        if (itemData.itemType != ItemData.ItemType.Note) return;

        // Если DiaryManager уже есть - скрываем записку если дневник не разблокирован
        if (DiaryManager.instance != null)
        {
            if (!DiaryManager.instance.IsDiaryUnlocked())
            {
                gameObject.SetActive(false);
                // Подписываемся на событие
                DiaryManager.instance.diaryUnlockedEvent += OnDiaryUnlocked;
            }
        }
    }

    private void OnDestroy()
    {
        if (DiaryManager.instance != null && itemData != null && itemData.itemType == ItemData.ItemType.Note)
        {
            DiaryManager.instance.diaryUnlockedEvent -= OnDiaryUnlocked;
        }
    }

    private void OnDiaryUnlocked()
    {
        if (gameObject != null) gameObject.SetActive(true);
    }

    public void Interact()
    {
        if (itemData == null) return;

        // Дневник - самый важный предмет, его должны моч подобрать всегда
        if (itemData.itemType == ItemData.ItemType.Diary)
        {
            Debug.Log("📖 Найден дневник!");
            
            // Пытаемся разблокировать дневник если он есть
            if (DiaryManager.instance != null)
            {
                DiaryManager.instance.UnlockDiary();
            }
            else
            {
                Debug.LogWarning("⚠️ DiaryManager не найден на сцене!");
            }
            
            TryUpdateQuest();
            Destroy(gameObject);
            return;
        }

        // Для остального нужен InventorySystem
        if (InventorySystemNew.instance == null) return;

        // Записка
        if (itemData.itemType == ItemData.ItemType.Note)
        {
            if (DiaryManager.instance != null && DiaryManager.instance.IsDiaryUnlocked())
                DiaryManager.instance.AddEntryByID(itemData.diaryEntryID);
            
            TryUpdateQuest();
            Destroy(gameObject);
            return;
        }

        // Остальные предметы
        bool success = InventorySystemNew.instance.AddItem(itemData, 1);
        if (success)
        {
            TryUpdateQuest();
            Destroy(gameObject);
        }
    }

    private void TryUpdateQuest()
    {
        if (!string.IsNullOrEmpty(questTextOnPickup) && QuestManager.instance != null)
            QuestManager.instance.UpdateQuest(questTextOnPickup);
    }
}