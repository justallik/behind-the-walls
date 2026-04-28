using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DiaryUI : MonoBehaviour
{
    public static DiaryUI instance;

    [Header("📄 UI Страницы (10 штук)")]
    [SerializeField] private GameObject[] pageUIs = new GameObject[10]; // Page_1 до Page_10

    [Header("📌 Маркеры (кнопки)")]
    [SerializeField] private GameObject[] allMarkerButtons = new GameObject[10]; // Marker_1 до Marker_10

    private int currentDisplayIndex = 0; // Индекс в отсортированном списке записей

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Скрываем все страницы
        for (int i = 0; i < pageUIs.Length; i++)
        {
            if (pageUIs[i] != null)
                pageUIs[i].SetActive(false);
        }

        if (DiaryManager.instance != null)
        {
            RefreshDiaryDisplay();
        }
        else
        {
            Debug.LogWarning("⚠️ DiaryManager не найден при инициализации DiaryUI!");
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        
        if (DiaryManager.instance == null) return; // ✅ Добавил проверку

        int totalEntries = DiaryManager.instance.GetTotalEntries();

        // Клавиши 1-9 для переключения между доступными записями
        if (Keyboard.current.digit1Key.wasPressedThisFrame && totalEntries > 0) ShowEntry(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && totalEntries > 1) ShowEntry(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && totalEntries > 2) ShowEntry(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame && totalEntries > 3) ShowEntry(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame && totalEntries > 4) ShowEntry(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame && totalEntries > 5) ShowEntry(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame && totalEntries > 6) ShowEntry(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame && totalEntries > 7) ShowEntry(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame && totalEntries > 8) ShowEntry(8);

        // 🎯 НОВОЕ: Закрытие дневника по Q или ESC
        if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (InventoryUINew.instance != null)
            {
                InventoryUINew.instance.ShowInventoryTab();
            }
        }
    }

    /// <summary>
    /// Показать запись по индексу в отсортированном списке
    /// </summary>
    public void ShowEntry(int displayIndex)
    {
        if (DiaryManager.instance == null) 
        {
            Debug.LogWarning("⚠️ DiaryManager не найден!");
            return;
        }

        var sortedEntries = DiaryManager.instance.GetSortedEntries();
        if (displayIndex < 0 || displayIndex >= sortedEntries.Count) return;

        DiaryEntry entry = sortedEntries[displayIndex];
        currentDisplayIndex = displayIndex;

        // 1. Скрываем АБСОЛЮТНО ВСЕ страницы перед показом нужной
        for (int i = 0; i < pageUIs.Length; i++)
        {
            if (pageUIs[i] != null)
                pageUIs[i].SetActive(false);
        }

        // 2. Вычисляем индекс страницы на основе ID записи
        int pageIndex = entry.id - 1;

        // Проверяем, что такой индекс есть в нашем массиве страниц
        if (pageIndex >= 0 && pageIndex < pageUIs.Length)
        {
            if (pageUIs[pageIndex] != null)
            {
                pageUIs[pageIndex].SetActive(true);
                
                // Обновляем текст (заголовок, дату и т.д.) если нужно
                UpdatePageContent(entry, pageIndex);
                
                entry.isNew = false; // Помечаем как прочитанную
                
                // 🔔 Обновляем только текст маркера для этой записи
                if (displayIndex >= 0 && displayIndex < allMarkerButtons.Length)
                {
                    GameObject marker = allMarkerButtons[displayIndex];
                    if (marker != null)
                    {
                        TextMeshProUGUI markerText = marker.GetComponentInChildren<TextMeshProUGUI>();
                        if (markerText != null)
                        {
                            // Меняем "NEW" обратно на ID
                            markerText.text = entry.id.ToString();
                            markerText.color = Color.white; // Возвращаем белый цвет
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Ячейка страницы под индексом {pageIndex} пуста в Инспекторе!");
            }
        }
    }

    /// <summary>
    /// Обновить содержимое страницы
    /// </summary>
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

        // Убираем "NEW" маркер
        entry.isNew = false;
    }

    /// <summary>
    /// Обновить видимость маркеров - показываем только столько сколько записей
    /// </summary>
    public void RefreshDiaryDisplay()
    {
        if (DiaryManager.instance == null) 
        {
            Debug.LogWarning("⚠️ DiaryManager не найден!");
            return;
        }

        // 1. Получаем список всех найденных записей, отсортированный по ID (1, 4, 7...)
        var sortedEntries = DiaryManager.instance.GetSortedEntries();

        // 2. Сначала скрываем ВСЕ маркеры
        for (int i = 0; i < allMarkerButtons.Length; i++)
        {
            if (allMarkerButtons[i] != null)
                allMarkerButtons[i].SetActive(false);
        }

        // 3. Проходимся по списку НАЙДЕННЫХ записей и включаем маркеры по порядку
        for (int i = 0; i < sortedEntries.Count; i++)
        {
            // Если записей больше, чем у нас есть кнопок-маркеров, выходим из цикла
            if (i >= allMarkerButtons.Length) break;

            DiaryEntry entry = sortedEntries[i];
            GameObject marker = allMarkerButtons[i];

            marker.SetActive(true); // Включаем кнопку (Layout Group сам подвинет её на i-тое место)

            // Устанавливаем текст на маркере: "NEW" если запись новая, иначе ID записи
            TextMeshProUGUI markerText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (markerText != null)
            {
                // Если запись новая - показываем "NEW", иначе - ID
                markerText.text = entry.isNew ? "NEW" : entry.id.ToString();
                
                // Опционально: можно менять цвет для новых записей
                if (entry.isNew)
                {
                    markerText.color = new Color(1f, 0.5f, 0f); // Оранжевый цвет для NEW
                }
                else
                {
                    markerText.color = Color.white; // Белый для обычных
                }
            }

            // Настраиваем нажатие
            Button btn = marker.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                int indexToShow = i; // Сохраняем индекс для вызова
                btn.onClick.AddListener(() => ShowEntry(indexToShow));
            }
        }

        // 4. Показываем первую доступную запись по умолчанию, если они есть
        if (sortedEntries.Count > 0)
        {
            ShowEntry(0);
        }
    }
}
