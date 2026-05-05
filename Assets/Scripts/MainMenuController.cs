using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Назви сцен")]
    [SerializeField] private string newGameScene = "IntroQuote";
    [SerializeField] private string continueScene = "SampleScene";

    public void NewGame()
    {
        SceneManager.LoadScene(newGameScene);
    }

    public void Continue()
    {
        // Поки що просто завантажує SampleScene
        // Пізніше тут буде завантаження SaveSystem
        SceneManager.LoadScene(continueScene);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
