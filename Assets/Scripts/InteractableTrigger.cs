using UnityEngine;

public class InteractableTrigger : MonoBehaviour
{
    [Header("Квест")]
    [SerializeField] private string questIdToIncrement;

    [Header("Интерактив")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"✋ Можно взаимодействовать с {gameObject.name} (нажми E)");
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey) && !hasInteracted)
        {
            Interact();
        }
    }

    private void Interact()
    {
        hasInteracted = true;

        if (QuestManager.instance == null)
        {
            Debug.LogError("❌ QuestManager не найден!");
            return;
        }

        Debug.Log($"🔍 Обыскиваем {gameObject.name}...");
        QuestManager.instance.IncrementQuestCounter(questIdToIncrement);
    }
}
