using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class IntroController : MonoBehaviour
{
    public static bool introPlaying = false;

    [Header("Note")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseMovement cameraController;
    [SerializeField] private PlayableDirector director;

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;

    [Header("Intro Elements")]
    [SerializeField] private GameObject blurOverlay;
    [SerializeField] private GameObject eyelidTop;
    [SerializeField] private GameObject eyelidBottom;
    [SerializeField] private GameObject introImage;

    private bool waitingForNoteRead = false;
    private bool noteIsOpen = false;
    private bool introFinished = false;

    private void Start()
    {
        if (SaveSystem.instance != null && SaveSystem.instance.SaveExists())
        {
            SkipIntro();
            return;
        }

        introPlaying = true;
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraController != null) cameraController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (notePanel != null) notePanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (hudRoot != null) hudRoot.SetActive(false);

        if (director != null) director.Play();
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

    public void ShowNoteHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            hintText.text = "[Space] — Прочитати записку";
        }
        if (director != null) director.Pause();
        waitingForNoteRead = true;
    }

    private void SkipIntro()
    {
        introPlaying = false;
        introFinished = true;

        if (director != null) director.Stop();

        // Ховаємо всі елементи інтро
        if (blurOverlay != null) blurOverlay.SetActive(false);
        if (eyelidTop != null) eyelidTop.SetActive(false);
        if (eyelidBottom != null) eyelidBottom.SetActive(false);
        if (introImage != null) introImage.SetActive(false);
        if (notePanel != null) notePanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.ResetSpeed();
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraController != null) cameraController.enabled = true;
        if (hudRoot != null) hudRoot.SetActive(true);

        if (SleepSystem.instance != null && SleepSystem.instance.fadeScreen != null)
        {
            SleepSystem.instance.fadeScreen.alpha = 0f;
            SleepSystem.instance.fadeScreen.gameObject.SetActive(false);
        }

        PlayerAnimator playerAnimator = FindFirstObjectByType<PlayerAnimator>();
        if (playerAnimator != null) playerAnimator.StandUp();
    }

    public void FinishIntro()
    {
        introPlaying = false;
        introFinished = true;

        if (playerMovement != null)
        {
            playerMovement.ResetSpeed();
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraController != null) cameraController.enabled = true;
        if (hudRoot != null) hudRoot.SetActive(true);

        QuestManager.instance?.ActivateQuest("quest_look_around");

        PlayerAnimator playerAnimator = FindFirstObjectByType<PlayerAnimator>();
        if (playerAnimator != null) playerAnimator.StandUp();
    }

    private void OpenNote()
    {
        noteIsOpen = true;
        if (notePanel != null) notePanel.SetActive(true);
        if (hintText != null) hintText.text = "[Space] — Закрити";
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