using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    public static GameOverController instance;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "SampleScene";

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;

    private string savePath => Application.persistentDataPath + "/save.json";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        MouseMovement mm = FindFirstObjectByType<MouseMovement>();
        if (mm != null) mm.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.unscaledDeltaTime;
                gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                yield return null;
            }
            gameOverCanvasGroup.alpha = 1f;
        }

        Time.timeScale = 0f;
    }

    public void ContinueFromSave()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}