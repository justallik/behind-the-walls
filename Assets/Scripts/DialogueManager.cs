using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Lines")]
    [SerializeField] private string[] lines;

    private int currentLine = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowNextLine()
    {
        if (currentLine >= lines.Length) return;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.text = lines[currentLine];
        currentLine++;
    }

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