using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ElevatorEscape : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private Transform exitPoint;

    [Header("UI")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private string hintMessage = "[F] — Вибратись";

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float pauseInDark = 1f;

    private bool playerInRange = false;
    private bool isEscaping = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        ShowHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        ShowHint(false);
    }

    private void Update()
    {
        if (!playerInRange || isEscaping) return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            StartCoroutine(EscapeRoutine());
    }

    private IEnumerator EscapeRoutine()
    {
        isEscaping = true;
        ShowHint(false);

        if (SleepSystem.instance != null && SleepSystem.instance.fadeScreen != null)
        {
            var fade = SleepSystem.instance.fadeScreen;
            fade.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(fade, 0, 1));
        }

        yield return new WaitForSeconds(pauseInDark);

        if (exitPoint != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                CharacterController cc = playerObj.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerObj.transform.position = exitPoint.position;
                playerObj.transform.rotation = exitPoint.rotation;
                if (cc != null) cc.enabled = true;
            }
        }

        if (SleepSystem.instance != null && SleepSystem.instance.fadeScreen != null)
        {
            var fade = SleepSystem.instance.fadeScreen;
            yield return StartCoroutine(Fade(fade, 1, 0));
            fade.gameObject.SetActive(false);
        }

        isEscaping = false;
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(CanvasGroup fade, float from, float to)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }
        fade.alpha = to;
    }

    private void ShowHint(bool show)
    {
        if (hintPanel != null) hintPanel.SetActive(show);
        if (hintText != null) hintText.text = hintMessage;
    }
}