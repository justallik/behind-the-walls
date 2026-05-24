using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string newGameScene = "IntroQuote";
    [SerializeField] private string continueScene = "SampleScene";

    private string savePath => Application.persistentDataPath + "/save.json";

    public void NewGame()
    {
        // Видаляємо збереження якщо є
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Збереження видалено — нова гра");
        }

        SceneManager.LoadScene(newGameScene);
    }

    public void Continue()
    {
        if (File.Exists(savePath))
        {
            SceneManager.LoadScene(continueScene);
        }
        else
        {
            Debug.LogWarning("Збереження не знайдено");
            // Тут можна показати повідомлення на екрані
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}