using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class NoteViewer : MonoBehaviour
{
    public static NoteViewer instance;

    [Header("Note Panels")]
    [SerializeField] private List<NotePanel> notePanels = new List<NotePanel>();

    [Header("Hint")]
    [SerializeField] private GameObject hintPanel;

    [Header("Cutscene")]
    [SerializeField] private SoletCutsceneController soletCutscene;

    private string questIdToCompleteOnClose;
    private string questIdToActivateOnClose;
    private bool isOpen = false;
    private bool playCutscene = false;
    private GameObject currentOpenPanel;

    [System.Serializable]
    public class NotePanel
    {
        public int diaryEntryID;
        public GameObject panel;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            Destroy(gameObject);

        foreach (var np in notePanels)
            if (np.panel != null) np.panel.SetActive(false);

        if (hintPanel != null) hintPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            CloseNote();
    }

    public void ShowNote(int diaryEntryID, string questToComplete, string questToActivate, bool withCutscene = false)
    {
        GameObject panelToShow = null;
        foreach (var np in notePanels)
        {
            if (np.diaryEntryID == diaryEntryID)
            {
                panelToShow = np.panel;
                break;
            }
        }

        if (panelToShow == null)
        {
            Debug.LogError($"Панель для записки #{diaryEntryID} не знайдена");
            ActivateQuests(questToComplete, questToActivate);
            if (withCutscene && soletCutscene != null)
                soletCutscene.PlayCutscene();
            return;
        }

        isOpen = true;
        playCutscene = withCutscene;
        questIdToCompleteOnClose = questToComplete;
        questIdToActivateOnClose = questToActivate;
        currentOpenPanel = panelToShow;

        panelToShow.SetActive(true);
        if (hintPanel != null) hintPanel.SetActive(true);
    }

    private void CloseNote()
    {
        isOpen = false;

        if (currentOpenPanel != null) currentOpenPanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);

        ActivateQuests(questIdToCompleteOnClose, questIdToActivateOnClose);

        if (playCutscene && soletCutscene != null)
            soletCutscene.PlayCutscene();

        questIdToCompleteOnClose = "";
        questIdToActivateOnClose = "";
        playCutscene = false;
        currentOpenPanel = null;
    }

    private void ActivateQuests(string questToComplete, string questToActivate)
    {
        if (QuestManager.instance == null) return;

        if (!string.IsNullOrEmpty(questToComplete))
            QuestManager.instance.CompleteQuest(questToComplete);

        if (!string.IsNullOrEmpty(questToActivate))
            QuestManager.instance.ActivateQuest(questToActivate);
    }
}