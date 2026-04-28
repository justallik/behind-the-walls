using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct InitialEntryData
{
    public int id;
}

public class DiaryManager : MonoBehaviour
{
    public static DiaryManager instance;

    // Храним записи по ID
    private Dictionary<int, DiaryEntry> entries = new Dictionary<int, DiaryEntry>();
    
    private bool diaryUnlocked = false;

    [Header("Стартовые записи")]
    [SerializeField] public List<InitialEntryData> startingEntries = new List<InitialEntryData>();

    // Событие - срабатывает когда дневник разблокирован
    public delegate void OnDiaryUnlocked();
    public event OnDiaryUnlocked diaryUnlockedEvent;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Загружаем стартовые записи (только по ID)
        foreach (var data in startingEntries)
        {
            // Создаем пустую запись - текст будет в картинке страницы
            entries[data.id] = new DiaryEntry
            {
                id = data.id,
                title = $"Запись #{data.id}",
                content = "", // Пусто - текст в UI странице
                date = "",
                isNew = false
            };
        }

        Debug.Log($"📖 Загружено {startingEntries.Count} начальных записей");
        
        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    /// <summary>
    /// Разблокировать дневник
    /// </summary>
    public void UnlockDiary()
    {
        diaryUnlocked = true;
        Debug.Log("📖 Дневник разблокирован!");
        
        // 🔔 Срабатываем событие - все записки активируются
        diaryUnlockedEvent?.Invoke();
        
        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    /// <summary>
    /// Добавить запись по ID
    /// </summary>
    public void AddEntry(int id, string title, string content, string date)
    {
        if (id < 1 || id > 9)
        {
            Debug.LogError($"❌ ID записи должен быть 1-9, получено: {id}");
            return;
        }

        entries[id] = new DiaryEntry
        {
            id = id,
            title = title,
            content = content,
            date = date,
            isNew = true
        };

        Debug.Log($"📝 Запись #{id} добавлена!");

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    /// <summary>
    /// Получить все записи отсортированные по ID
    /// </summary>
    public List<DiaryEntry> GetSortedEntries()
    {
        List<DiaryEntry> sorted = new List<DiaryEntry>(entries.Values);
        sorted.Sort((a, b) => a.id.CompareTo(b.id));
        return sorted;
    }

    /// <summary>
    /// Получить запись по индексу в отсортированном списке (0 = первая доступная)
    /// </summary>
    public DiaryEntry GetEntryByDisplayIndex(int displayIndex)
    {
        List<DiaryEntry> sorted = GetSortedEntries();
        if (displayIndex < 0 || displayIndex >= sorted.Count) return null;
        return sorted[displayIndex];
    }

    public int GetTotalEntries() => entries.Count;
    public bool IsDiaryUnlocked() => diaryUnlocked;
    public bool IsUnlocked() => diaryUnlocked; // Алиас для совместимости

    /// <summary>
    /// Добавить запись только по ID (для предметов типа Note)
    /// Текст берется из starting entries
    /// </summary>
    public void AddEntryByID(int entryID)
    {
        if (entryID < 1 || entryID > 9)
        {
            Debug.LogError($"❌ Неверный ID записи: {entryID}");
            return;
        }

        if (!diaryUnlocked)
        {
            Debug.LogWarning("⚠️ Дневник еще не разблокирован, запись не будет добавлена!");
            return;
        }

        // Проверяем, есть ли уже такая запись
        if (entries.ContainsKey(entryID))
        {
            Debug.LogWarning($"⚠️ Запись #{entryID} уже добавлена!");
            return;
        }

        // Просто помечаем что запись найдена (создаем пустую запись с пустыми данными)
        // Реальные текст/картинка отображаются через Page UI
        entries[entryID] = new DiaryEntry
        {
            id = entryID,
            title = $"Запись #{entryID}",
            content = "", // Пусто - текст в картинке
            date = "",
            isNew = true
        };

        Debug.Log($"📖 Запись #{entryID} найдена!");

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }
}
