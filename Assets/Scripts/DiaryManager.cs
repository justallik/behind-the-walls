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

    private Dictionary<int, DiaryEntry> entries = new Dictionary<int, DiaryEntry>();
    private bool diaryUnlocked = false;

    [Header("Starting Entries")]
    [SerializeField] public List<InitialEntryData> startingEntries = new List<InitialEntryData>();

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
        foreach (var data in startingEntries)
        {
            entries[data.id] = new DiaryEntry
            {
                id = data.id,
                title = $"Запис #{data.id}",
                content = "",
                date = "",
                isNew = false
            };
        }

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    public void UnlockDiary()
    {
        diaryUnlocked = true;
        Debug.Log("Щоденник розблоковано");
        diaryUnlockedEvent?.Invoke();

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    public void AddEntry(int id, string title, string content, string date)
    {
        if (id < 1 || id > 14)
        {
            Debug.LogError($"ID запису має бути від 1 до 14, отримано: {id}");
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

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }

    public List<DiaryEntry> GetSortedEntries()
    {
        List<DiaryEntry> sorted = new List<DiaryEntry>(entries.Values);
        sorted.Sort((a, b) => a.id.CompareTo(b.id));
        return sorted;
    }

    public DiaryEntry GetEntryByDisplayIndex(int displayIndex)
    {
        List<DiaryEntry> sorted = GetSortedEntries();
        if (displayIndex < 0 || displayIndex >= sorted.Count) return null;
        return sorted[displayIndex];
    }

    public int GetTotalEntries() => entries.Count;
    public bool IsDiaryUnlocked() => diaryUnlocked;
    public bool IsUnlocked() => diaryUnlocked;

    public void AddEntryByID(int entryID)
    {
        if (entryID < 1 || entryID > 14)
        {
            Debug.LogError($"Невірний ID запису: {entryID}");
            return;
        }

        if (!diaryUnlocked)
        {
            Debug.LogWarning("Щоденник ще не розблоковано");
            return;
        }

        if (entries.ContainsKey(entryID))
        {
            Debug.LogWarning($"Запис #{entryID} вже додано");
            return;
        }

        entries[entryID] = new DiaryEntry
        {
            id = entryID,
            title = $"Запис #{entryID}",
            content = "",
            date = "",
            isNew = true
        };

        Debug.Log($"Запис #{entryID} знайдено");

        if (DiaryUI.instance != null)
            DiaryUI.instance.RefreshDiaryDisplay();
    }
}