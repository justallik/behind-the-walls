// using UnityEngine;
// using TMPro;
// using System.Collections;

// public class HintManager : MonoBehaviour
// {
//     public static HintManager instance;

//     [Header("UI")]
//     [SerializeField] private GameObject hintPanel;
//     [SerializeField] private TextMeshProUGUI hintTextUI;

//     [Header("Settings")]
//     [SerializeField] private float defaultDuration = 3f;

//     private Coroutine hideCoroutine;

//     private void Awake()
//     {
//         if (instance == null)
//             instance = this;
//         else
//             Destroy(gameObject);
//     }

//     public void ShowHint(string text, float duration = -1f)
//     {
//         if (hintPanel == null || hintTextUI == null) return;

//         hintTextUI.text = text;
//         hintPanel.SetActive(true);

//         if (hideCoroutine != null) StopCoroutine(hideCoroutine);

//         float dur = duration > 0 ? duration : defaultDuration;
//         hideCoroutine = StartCoroutine(HideAfterDelay(dur));
//     }

//     private IEnumerator HideAfterDelay(float duration)
//     {
//         yield return new WaitForSeconds(duration);
//         if (hintPanel != null) hintPanel.SetActive(false);
//     }
// }

using UnityEngine;
using TMPro;
using System.Collections;

public class HintManager : MonoBehaviour
{
    public static HintManager instance;

    [Header("UI")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintTextUI;

    [Header("Settings")]
    [SerializeField] private float defaultDuration = 3f;

    private Coroutine hideCoroutine;

    // Priority lock: while locked, normal ShowHint calls are ignored.
    private bool isLocked;
    private object lockOwner;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Sets a fixed hint text and blocks other scripts from overwriting it.
    public void LockHint(object owner, string text)
    {
        isLocked = true;
        lockOwner = owner;

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        if (hintTextUI != null) hintTextUI.text = text;
        if (hintPanel != null) hintPanel.SetActive(true);
    }

    // Releases the lock. Only the owner that locked it can unlock.
    public void UnlockHint(object owner)
    {
        if (!isLocked || lockOwner != owner) return;

        isLocked = false;
        lockOwner = null;

        if (hintPanel != null) hintPanel.SetActive(false);
    }

    public void ShowHint(string text, float duration = -1f)
    {
        if (isLocked) return; // while locked nothing can overwrite the text

        if (hintPanel == null || hintTextUI == null) return;

        hintTextUI.text = text;
        hintPanel.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        float dur = duration > 0 ? duration : defaultDuration;
        hideCoroutine = StartCoroutine(HideAfterDelay(dur));
    }

    private IEnumerator HideAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!isLocked && hintPanel != null) hintPanel.SetActive(false);
    }
}
