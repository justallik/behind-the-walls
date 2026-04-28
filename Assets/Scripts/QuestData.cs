using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Quest/Create Simple Quest", order = 1)]
public class QuestData : ScriptableObject
{
    [Header("Основное")]
    public string questId = "quest_001";
    
    [TextArea(2, 4)]
    public string questObjective = "Найдите деревню"; // ЭТО главное! Просто текст задания
    
    [Header("Статус")]
    public bool isActive = false;
    public bool isCompleted = false;
    
    [Header("Счётчик (опционально)")]
    public bool useCounter = false; // Нужен ли счётчик типа (0/3)?
    public int currentCount = 0;
    public int maxCount = 1;

    public void Initialize()
    {
        isActive = false;
        isCompleted = false;
        currentCount = 0;
    }

    public void ActivateQuest()
    {
        if (!isActive && !isCompleted)
        {
            isActive = true;
            currentCount = 0;
            Debug.Log($"🎯 ЗАДАНИЕ: {GetFullObjective()}");
        }
    }

    public void IncrementCounter()
    {
        if (useCounter && currentCount < maxCount)
        {
            currentCount++;
            Debug.Log($"📍 {GetFullObjective()}");
            
            if (currentCount >= maxCount)
            {
                CompleteQuest();
            }
        }
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        isActive = false;
        Debug.Log($"✅ ЗАДАНИЕ ВЫПОЛНЕНО!");
    }

    public string GetFullObjective()
    {
        if (useCounter)
            return $"{questObjective} ({currentCount}/{maxCount})";
        else
            return questObjective;
    }
}

