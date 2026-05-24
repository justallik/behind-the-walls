using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string line;
    [SerializeField] private float hideAfterSeconds = 4f;

    [Header("Voice")]
    [SerializeField] private AudioClip voiceClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Invisible Wall")]
    [SerializeField] private GameObject invisibleWall;

    [Header("Quest")]
    [SerializeField] private bool completeQuest;
    [SerializeField] private string completeQuestId;
    [SerializeField] private bool activateQuest;
    [SerializeField] private string activateQuestId;

    private bool _triggered;

    private void Start()
    {
        if (invisibleWall != null)
            invisibleWall.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        if (invisibleWall != null)
            invisibleWall.SetActive(false);

        if (!string.IsNullOrEmpty(line))
        {
            DialogueManager.Instance.ShowLine(line);

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