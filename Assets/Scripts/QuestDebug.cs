using UnityEngine;

public class QuestDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== QUEST DEBUG ===");

        // Проверяем QuestManager
        if (QuestManager.instance == null)
        {
            Debug.LogError("❌ QuestManager НЕ НАЙДЕН!");
            return;
        }
        Debug.Log("✅ QuestManager найден");

        // Пытаемся загрузить квест
        QuestData quest = Resources.Load<QuestData>("Quests/1_Awakening");
        if (quest == null)
        {
            Debug.LogError("❌ Квест 'Quests/1_Awakening' НЕ НАЙДЕН в Resources!");
            Debug.Log("📁 Проверь: Assets/Resources/Quests/ - там должен быть файл '1_Awakening.asset'");
            return;
        }
        Debug.Log($"✅ Квест загружен: {quest.questId} - '{quest.questObjective}'");

        // Активируем квест
        Debug.Log("🎯 Активируем квест...");
        QuestManager.instance.ActivateQuest("quest_awakening");
        Debug.Log("✅ ActivateQuest() вызван");
    }
}
