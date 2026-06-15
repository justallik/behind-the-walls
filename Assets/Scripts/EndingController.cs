using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EndingController : MonoBehaviour
{
    public static EndingController instance;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    [Header("Eyelids")]
    [SerializeField] private RectTransform eyelidTop;
    [SerializeField] private RectTransform eyelidBottom;

    private float eyelidTopClosed    = -292f;
    private float eyelidBottomClosed =  212f;
    private float eyelidTopOpen      =  464f;
    private float eyelidBottomOpen   = -496f;

    [Header("Blur")]
    [SerializeField] private CanvasGroup blurOverlay;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string endingLine = "Noa...";
    [SerializeField] private float dialogueHoldDuration = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip voiceClip;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeScreen;

    [Header("Final Image")]
    [SerializeField] private CanvasGroup finalImage;
    [SerializeField] private float finalImageFadeDuration = 0f;

    [Header("Timings")]
    [SerializeField] private float closedEyelidsDelay = 2f;
    [SerializeField] private float eyeOpenDuration   = 2.5f;
    [SerializeField] private float blurFadeDuration  = 3.8f;
    [SerializeField] private float pauseBeforeDialog = 0.5f;
    [SerializeField] private float pauseAfterDialog  = 1f;

    private void Awake() => instance = this;

    private void Start()
    {
        if (eyelidTop != null) eyelidTop.gameObject.SetActive(false);
        if (eyelidBottom != null) eyelidBottom.gameObject.SetActive(false);
        if (blurOverlay != null) { blurOverlay.alpha = 0f; blurOverlay.gameObject.SetActive(false); }
        if (fadeScreen != null) { fadeScreen.alpha = 0f; fadeScreen.gameObject.SetActive(false); }
        if (finalImage != null) { finalImage.alpha = 0f; finalImage.gameObject.SetActive(false); }
    }

    public void StartEnding()
    {
        if (SleepSystem.instance != null)
            SleepSystem.instance.HideFade();

        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        MouseMovement mm = FindFirstObjectByType<MouseMovement>();
        if (mm != null)
        {
            mm.SetXRotation(-90f);
            mm.enabled = false;
        }

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (eyelidTop != null) { eyelidTop.gameObject.SetActive(true); SetEyelids(eyelidTopClosed, eyelidBottomClosed); }
        if (eyelidBottom != null) eyelidBottom.gameObject.SetActive(true);
        if (blurOverlay != null) { blurOverlay.gameObject.SetActive(true); blurOverlay.alpha = 1f; }

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true);
            fadeScreen.alpha = 1f;
            StartCoroutine(FadeCanvas(fadeScreen, 1f, 0f, 1f));
        }

        StartCoroutine(FadeCanvas(blurOverlay, 1f, 0f, blurFadeDuration));
        yield return new WaitForSeconds(closedEyelidsDelay);
        yield return StartCoroutine(AnimateEyelids(eyelidTopClosed, eyelidTopOpen, eyelidBottomClosed, eyelidBottomOpen, eyeOpenDuration));
        yield return new WaitForSeconds(pauseBeforeDialog);

        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);

        if (dialogueManager != null)
        {
            dialogueManager.ShowLine(endingLine);
            yield return new WaitForSeconds(dialogueHoldDuration);
            dialogueManager.HideDialogue();
        }
        else
        {
            float waitTime = (voiceClip != null) ? voiceClip.length : dialogueHoldDuration;
            yield return new WaitForSeconds(waitTime);
        }

        yield return new WaitForSeconds(pauseAfterDialog);

        if (finalImage != null)
        {
            finalImage.gameObject.SetActive(true);
            if (finalImageFadeDuration > 0)
                yield return StartCoroutine(FadeCanvas(finalImage, 0f, 1f, finalImageFadeDuration));
            else
                finalImage.alpha = 1f;
        }

        yield return new WaitUntil(() =>
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame
        );

        SceneManager.LoadScene("MainMenu");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void SetEyelids(float topY, float bottomY)
    {
        if (eyelidTop != null)
            eyelidTop.anchoredPosition = new Vector2(eyelidTop.anchoredPosition.x, topY);
        if (eyelidBottom != null)
            eyelidBottom.anchoredPosition = new Vector2(eyelidBottom.anchoredPosition.x, bottomY);
    }

    private IEnumerator AnimateEyelids(float topFrom, float topTo, float botFrom, float botTo, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            if (eyelidTop != null)
                eyelidTop.anchoredPosition = new Vector2(eyelidTop.anchoredPosition.x, Mathf.Lerp(topFrom, topTo, t));
            if (eyelidBottom != null)
                eyelidBottom.anchoredPosition = new Vector2(eyelidBottom.anchoredPosition.x, Mathf.Lerp(botFrom, botTo, t));
            yield return null;
        }
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float timer = 0f;
        cg.alpha = from;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}