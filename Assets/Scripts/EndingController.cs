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
    [SerializeField] private string endingLine = "НОА...";
    [SerializeField] private float dialogueHoldDuration = 3f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeScreen;

    [Header("Ending Text")]
    [SerializeField] private CanvasGroup endingTextGroup;
    [SerializeField] private float textFadeInDuration = 2f;
    [SerializeField] private GameObject pressSpaceHint;

    [Header("Timings")]
    [SerializeField] private float eyeOpenDuration   = 2.5f;
    [SerializeField] private float blurFadeDuration  = 3.8f;
    [SerializeField] private float pauseBeforeDialog = 1f;
    [SerializeField] private float pauseAfterDialog  = 2f;

    private void Awake() => instance = this;

    private void Start()
    {
        if (eyelidTop != null)       eyelidTop.gameObject.SetActive(false);
        if (eyelidBottom != null)    eyelidBottom.gameObject.SetActive(false);
        if (blurOverlay != null)     { blurOverlay.alpha = 0f;     blurOverlay.gameObject.SetActive(false); }
        if (fadeScreen != null)      { fadeScreen.alpha = 0f;      fadeScreen.gameObject.SetActive(false); }
        if (endingTextGroup != null) { endingTextGroup.alpha = 0f; endingTextGroup.gameObject.SetActive(false); }
        if (pressSpaceHint != null)  pressSpaceHint.SetActive(false);
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

        if (eyelidTop != null)    { eyelidTop.gameObject.SetActive(true);   SetEyelids(eyelidTopClosed, eyelidBottomClosed); }
        if (eyelidBottom != null)   eyelidBottom.gameObject.SetActive(true);
        if (blurOverlay != null)  { blurOverlay.gameObject.SetActive(true); blurOverlay.alpha = 1f; }

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
        yield return StartCoroutine(AnimateEyelids(
            eyelidTopClosed, eyelidTopOpen,
            eyelidBottomClosed, eyelidBottomOpen,
            eyeOpenDuration
        ));

        yield return new WaitForSeconds(pauseBeforeDialog);

        if (dialogueManager != null)
        {
            dialogueManager.ShowLine(endingLine);
            yield return new WaitForSeconds(dialogueHoldDuration);
            dialogueManager.HideDialogue();
        }

        yield return new WaitForSeconds(pauseAfterDialog);

        if (fadeScreen != null)
            fadeScreen.alpha = 1f;

        yield return new WaitForSeconds(0.5f);

        if (endingTextGroup != null)
        {
            endingTextGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvas(endingTextGroup, 0f, 1f, textFadeInDuration));
        }

        yield return new WaitForSeconds(5f);

        if (pressSpaceHint != null)
            pressSpaceHint.SetActive(true);

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
                eyelidTop.anchoredPosition = new Vector2(
                    eyelidTop.anchoredPosition.x,
                    Mathf.Lerp(topFrom, topTo, t)
                );
            if (eyelidBottom != null)
                eyelidBottom.anchoredPosition = new Vector2(
                    eyelidBottom.anchoredPosition.x,
                    Mathf.Lerp(botFrom, botTo, t)
                );

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