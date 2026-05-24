using UnityEngine;
using TMPro;
using System.Collections;

public class HintTrigger : MonoBehaviour
{
    [Header("Hint Text")]
    [TextArea(2, 6)]
    [SerializeField] private string hintText = "[W,A,S,D] — Рухатись";

    [Header("UI")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintTextUI;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool hideOnExit = false;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float startDelay = 0.5f;

    private bool hasTriggered = false;
    private Coroutine hideCoroutine;
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        StartCoroutine(CheckPlayerInsideOnStart());
    }

    private IEnumerator CheckPlayerInsideOnStart()
    {
        yield return new WaitForSeconds(startDelay);

        if (hasTriggered) yield break;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) yield break;

        if (triggerCollider != null)
        {
            Vector3 playerPos = playerObj.transform.position;
            if (triggerCollider.bounds.Contains(playerPos))
            {
                hasTriggered = true;
                ShowHint();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;
        ShowHint();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!hideOnExit) return;

        HideHint();
    }

    private void ShowHint()
    {
        if (hintPanel == null || hintTextUI == null) return;

        hintTextUI.text = hintText;
        hintPanel.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        if (displayDuration > 0)
            hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void HideHint()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        if (hintPanel != null) hintPanel.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (hintPanel != null) hintPanel.SetActive(false);
    }
}