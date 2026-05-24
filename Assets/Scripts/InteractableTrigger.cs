using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableTrigger : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private string questIdToIncrement;

    private bool playerInRange = false;
    private bool hasInteracted = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame && !hasInteracted)
        {
            Interact();
        }
    }

    private void Interact()
    {
        hasInteracted = true;

        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager не знайдено");
            return;
        }

        QuestManager.instance.IncrementQuestCounter(questIdToIncrement);
    }
}