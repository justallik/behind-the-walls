using UnityEngine;
using UnityEngine.SceneManagement;

public class QuoteController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "SampleScene";

    public void HideQuoteAndNext()
    {
        Invoke(nameof(LoadNextScene), 0.5f);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
