using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Діалог")]
    [SerializeField] private string line;
    [SerializeField] private float hideAfterSeconds = 4f;

    [Header("Озвучка")]
    [SerializeField] private AudioClip voiceClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Квести")]
    [SerializeField] private bool completeQuest;
    [SerializeField] private string completeQuestId;
    [SerializeField] private bool activateQuest;
    [SerializeField] private string activateQuestId;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        if (!string.IsNullOrEmpty(line))
        {
            DialogueManager.Instance.ShowLine(line);

            // Якщо є озвучка — ховаємо після кліпу, інакше по таймеру
            if (voiceClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(voiceClip);
                Invoke(nameof(Hide), voiceClip.length + 0.3f);
            }
            else
            {
                Invoke(nameof(Hide), hideAfterSeconds);
            }
        }

        if (completeQuest && !string.IsNullOrEmpty(completeQuestId))
            QuestManager.instance.CompleteQuest(completeQuestId);

        if (activateQuest && !string.IsNullOrEmpty(activateQuestId))
            QuestManager.instance.ActivateQuest(activateQuestId);
    }

    private void Hide() => DialogueManager.Instance.HideDialogue();
}
