using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class IntroController : MonoBehaviour
{
    public static bool introPlaying = false;

    [Header("Записка")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Ссылки")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Mous1111 cameraController;
    [SerializeField] private PlayableDirector director;

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;

    private bool waitingForNoteRead = false;
    private bool noteIsOpen = false;
    private bool introFinished = false;

    private void Start()
    {
        // Якщо інтро вже було — одразу пропускаємо
        if (SaveSystem.instance != null && SaveSystem.instance.SaveExists())
        {
            SkipIntro();
            return;
        }

        introPlaying = true;
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraController != null) cameraController.enabled = false;

        if (notePanel != null) notePanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (hudRoot != null) hudRoot.SetActive(false);
    }

    private void Update()
    {
        if (introFinished) return;

        if (waitingForNoteRead && Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!noteIsOpen) OpenNote();
            else CloseNote();
        }
    }

    // ========== Вызывается через Signal с Timeline ==========
    public void ShowNoteHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            hintText.text = "[E] — Прочитать записку";
        }
        if (director != null) director.Pause();
        waitingForNoteRead = true;
    }

    private void SkipIntro()
    {
        introPlaying = false;
        introFinished = true;
        if (director != null) director.Stop();
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraController != null) cameraController.enabled = true;
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    public void FinishIntro()
    {
        introPlaying = false;
        introFinished = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraController != null) cameraController.enabled = true;
        if (hudRoot != null) hudRoot.SetActive(true);
        QuestManager.instance?.ActivateQuest("quest_look_around");
    }

    // ========== Записка ==========
    private void OpenNote()
    {
        noteIsOpen = true;
        if (notePanel != null) notePanel.SetActive(true);
        if (hintText != null) hintText.text = "[E] — Закрыть";
    }

    private void CloseNote()
    {
        noteIsOpen = false;
        waitingForNoteRead = false;
        if (notePanel != null) notePanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);

        if (director != null) director.Resume();
    }
}
