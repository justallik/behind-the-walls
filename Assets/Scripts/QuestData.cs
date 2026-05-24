using UnityEngine;

[CreateAssetMenu(fileName = "Quest_", menuName = "Quest/Create Simple Quest", order = 1)]
public class QuestData : ScriptableObject
{
    [Header("Main")]
    public string questId = "quest_001";

    [TextArea(2, 4)]
    public string questObjective = "Знайдіть село";

    [Header("Status")]
    public bool isActive = false;
    public bool isCompleted = false;

    [Header("Counter")]
    public bool useCounter = false;
    public int currentCount = 0;
    public int maxCount = 1;

    [Header("Quest Chain")]
    public string nextQuestId;

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
            Debug.Log($"Завдання: {GetFullObjective()}");
        }
    }

    public void IncrementCounter()
    {
        if (useCounter && currentCount < maxCount)
        {
            currentCount++;
            Debug.Log($"Прогрес: {GetFullObjective()}");
        }
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        isActive = false;
        Debug.Log($"Завдання виконано: {questId}");
    }

    public string GetFullObjective()
    {
        if (useCounter)
            return $"{questObjective} ({currentCount}/{maxCount})";
        else
            return questObjective;
    }
}