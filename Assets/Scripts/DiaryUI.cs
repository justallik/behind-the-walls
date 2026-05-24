using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DiaryUI : MonoBehaviour
{
    public static DiaryUI instance;

    [Header("Page UIs")]
    [SerializeField] private GameObject[] pageUIs = new GameObject[10];

    [Header("Marker Buttons")]
    [SerializeField] private GameObject[] allMarkerButtons = new GameObject[10];

    [Header("Note Slots")]
    [SerializeField] private GameObject[] noteSlots = new GameObject[4];

    private int currentDisplayIndex = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < pageUIs.Length; i++)
        {
            if (pageUIs[i] != null)
                pageUIs[i].SetActive(false);
        }

        if (DiaryManager.instance != null)
            RefreshDiaryDisplay();
        else
            Debug.LogError("DiaryManager не знайдено");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (DiaryManager.instance == null) return;

        int totalEntries = DiaryManager.instance.GetTotalEntries();

        if (Keyboard.current.digit1Key.wasPressedThisFrame && totalEntries > 0) ShowEntry(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && totalEntries > 1) ShowEntry(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && totalEntries > 2) ShowEntry(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame && totalEntries > 3) ShowEntry(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame && totalEntries > 4) ShowEntry(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame && totalEntries > 5) ShowEntry(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame && totalEntries > 6) ShowEntry(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame && totalEntries > 7) ShowEntry(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame && totalEntries > 8) ShowEntry(8);

        if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (InventoryUI.instance != null)
                InventoryUI.instance.ShowInventoryTab();
        }
    }

    public void ShowEntry(int displayIndex)
    {
        if (DiaryManager.instance == null) return;

        var sortedEntries = DiaryManager.instance.GetSortedEntries();
        if (displayIndex < 0 || displayIndex >= sortedEntries.Count) return;

        DiaryEntry entry = sortedEntries[displayIndex];
        currentDisplayIndex = displayIndex;

        for (int i = 0; i < pageUIs.Length; i++)
        {
            if (pageUIs[i] != null)
                pageUIs[i].SetActive(false);
        }

        int pageIndex = entry.id <= 10 ? entry.id - 1 : 9;

        if (pageIndex >= 0 && pageIndex < pageUIs.Length)
        {
            if (pageUIs[pageIndex] != null)
            {
                pageUIs[pageIndex].SetActive(true);
                UpdatePageContent(entry, pageIndex);
                entry.isNew = false;

                if (displayIndex >= 0 && displayIndex < allMarkerButtons.Length)
                {
                    GameObject marker = allMarkerButtons[displayIndex];
                    if (marker != null)
                    {
                        TextMeshProUGUI markerText = marker.GetComponentInChildren<TextMeshProUGUI>();
                        if (markerText != null)
                        {
                            markerText.text = entry.id.ToString();
                            markerText.color = Color.white;
                        }
                    }
                }

                if (pageIndex == 9)
                    RefreshNoteSlots();
            }
            else
            {
                Debug.LogWarning($"Сторінка під індексом {pageIndex} порожня в інспекторі");
            }
        }
    }

    private void RefreshNoteSlots()
    {
        for (int i = 0; i < noteSlots.Length; i++)
        {
            if (noteSlots[i] == null) continue;
            int noteId = 11 + i;
            bool hasEntry = DiaryManager.instance.GetSortedEntries()
                .Exists(e => e.id == noteId);
            noteSlots[i].SetActive(hasEntry);
        }
    }

    private void UpdatePageContent(DiaryEntry entry, int pageIndex)
    {
        if (DiaryManager.instance == null) return;

        GameObject page = pageUIs[pageIndex];
        if (page == null) return;

        TextMeshProUGUI titleText = page.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI contentText = page.transform.Find("Content")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dateText = page.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI counterText = page.transform.Find("Counter")?.GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = $"# {entry.id} - {entry.title}";
        if (contentText != null) contentText.text = entry.content;
        if (dateText != null) dateText.text = entry.date;

        int total = DiaryManager.instance.GetTotalEntries();
        if (counterText != null) counterText.text = $"{currentDisplayIndex + 1} of {total}";

        entry.isNew = false;
    }

    public void RefreshDiaryDisplay()
    {
        if (DiaryManager.instance == null) return;

        var sortedEntries = DiaryManager.instance.GetSortedEntries();

        for (int i = 0; i < allMarkerButtons.Length; i++)
        {
            if (allMarkerButtons[i] != null)
                allMarkerButtons[i].SetActive(false);
        }

        for (int i = 0; i < sortedEntries.Count; i++)
        {
            if (i >= allMarkerButtons.Length) break;

            DiaryEntry entry = sortedEntries[i];
            GameObject marker = allMarkerButtons[i];

            marker.SetActive(true);

            TextMeshProUGUI markerText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (markerText != null)
            {
                markerText.text = entry.isNew ? "NEW" : entry.id.ToString();
                markerText.color = entry.isNew ? new Color(1f, 0.5f, 0f) : Color.white;
            }

            Button btn = marker.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                int indexToShow = i;
                btn.onClick.AddListener(() => ShowEntry(indexToShow));
            }
        }

        if (sortedEntries.Count > 0)
            ShowEntry(0);
    }
}