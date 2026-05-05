using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [Header("Панель")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Кнопки")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnQuit;

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
            btnQuit.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        // ESC открывает/закрывает меню паузы
        // Но только если инвентарь закрыт
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC нажат! isPaused = " + isPaused);
            
            if (InventoryUINew.instance != null && InventoryUINew.instance.IsOpen())
            {
                InventoryUINew.instance.CloseInventory();
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
        // Time.timeScale = 0f;  // временно убрали
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
        {
            SaveSystem.instance.Save();
            Debug.Log("✅ Игра сохранена!");
        }
        else
        {
            Debug.LogError("❌ SaveSystem не найден!");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("👋 Выход из игры...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsPaused() => isPaused;
}
