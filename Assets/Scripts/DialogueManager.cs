using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Реплики (для Timeline)")]
    [SerializeField] private string[] lines;

    private int currentLine = 0;

    void Awake()
    {
        Instance = this;
    }

    // Для Timeline сигналів (як раніше)
    public void ShowNextLine()
    {
        if (currentLine >= lines.Length) return;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.text = lines[currentLine];
        currentLine++;
    }

    // Для геймплейних тригерів
    public void ShowLine(string text)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.text = text;
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void ResetDialogue()
    {
        currentLine = 0;
    }
}
