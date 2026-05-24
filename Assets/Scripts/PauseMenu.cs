using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [Header("Panel")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Buttons")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnQuit;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (btnContinue != null)
            btnContinue.onClick.AddListener(Continue);

        if (btnSave != null)
            btnSave.onClick.AddListener(SaveGame);

        if (btnQuit != null)
            btnQuit.onClick.AddListener(QuitToMainMenu);
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (InventoryUI.instance != null && InventoryUI.instance.IsOpen())
            {
                InventoryUI.instance.CloseInventory();
                return;
            }

            if (isPaused) Continue();
            else OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continue()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveGame()
    {
        if (SaveSystem.instance != null)
            SaveSystem.instance.Save();
        else
            Debug.LogError("SaveSystem не знайдено");
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public bool IsPaused() => isPaused;
}