using UnityEngine;

public class CorpseInteractable : MonoBehaviour
{
    [Header("Записка")]
    public string diaryEntryID;        // ID записи в DiaryManager

    [Header("Квест")]
    public string questIdToComplete;
    public string questIdToActivate;

    [Header("Настройки")]
    public float interactRange = 2f;
    public string interactHint = "E — обыскать тело";

    private bool hasBeenLooted = false;
    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (hasBeenLooted || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            Loot();
        }
    }

    private void Loot()
    {
        hasBeenLooted = true;

        Debug.Log("🧟 Обыскиваем тело...");

        // Добавляем запись в дневник
        if (!string.IsNullOrEmpty(diaryEntryID) && DiaryManager.instance != null)
        {
            if (int.TryParse(diaryEntryID, out int entryIdInt))
            {
                DiaryManager.instance.AddEntryByID(entryIdInt);
                Debug.Log($"📄 Найдена записка: {diaryEntryID}");
            }
            else
            {
                Debug.LogError($"❌ diaryEntryID должен быть числом, получено: {diaryEntryID}");
            }
        }

        // Квесты
        if (QuestManager.instance != null)
        {
            if (!string.IsNullOrEmpty(questIdToComplete))
                QuestManager.instance.CompleteQuest(questIdToComplete);

            if (!string.IsNullOrEmpty(questIdToActivate))
                QuestManager.instance.ActivateQuest(questIdToActivate);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
