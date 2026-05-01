using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class ItemContextMenu : MonoBehaviour
{
    public static ItemContextMenu instance;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button hotbarButton;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Hint Panel (подсказка для хотбара)")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Vector2 hintPanelOffset = new Vector2(5, 0); // смещение: X = отступ от меню, Y = смещение от центра

    private ItemData selectedItem = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("✅ ItemContextMenu.instance инициализирована");
            Debug.Log($"   GameObject: {gameObject.name}, Active: {gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("⚠️ Несколько ItemContextMenu на сцене, оставляю первую");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("🔴 ItemContextMenu.Start() ВЫЗВАНА");
        Debug.Log($"   gameObject.activeSelf: {gameObject.activeSelf}");
        Debug.Log($"   gameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        
        if (menuPanel == null)
            menuPanel = gameObject;
        
        if (canvasGroup == null)
        {
            canvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = menuPanel.AddComponent<CanvasGroup>();
        }
        
        // 🔍 ПОИСК КНОПОК - несколько способов
        if (useButton == null)
        {
            // Способ 1: Прямой поиск по имени
            Transform btn = transform.Find("UseButton");
            if (btn == null) btn = transform.Find("Use Button"); // с пробелом
            
            // Способ 2: Поиск во всех дочерних элементах
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Use"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) useButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 UseButton найден: {(useButton != null ? "✅ " + useButton.gameObject.name : "❌")}");
        }
        
        if (dropButton == null)
        {
            Transform btn = transform.Find("DropButton");
            if (btn == null) btn = transform.Find("Drop Button");
            
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Drop"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) dropButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 DropButton найден: {(dropButton != null ? "✅ " + dropButton.gameObject.name : "❌")}");
        }

        if (hotbarButton == null)
        {
            Transform btn = transform.Find("HotbarButton");
            if (btn == null) btn = transform.Find("Hotbar Button");
            
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Hotbar"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) hotbarButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 HotbarButton найден: {(hotbarButton != null ? "✅ " + hotbarButton.gameObject.name : "❌")}");
        }

        Debug.Log($"═══ LISTENER'ЫМ ═══");
        if (useButton) 
        {
            useButton.onClick.AddListener(OnUseClicked);
            Debug.Log("✅ OnUseClicked listener добавлен на " + useButton.gameObject.name);
        }
        else Debug.LogError("❌ useButton NULL!");
        
        if (dropButton) 
        {
            dropButton.onClick.AddListener(OnDropClicked);
            Debug.Log("✅ OnDropClicked listener добавлен на " + dropButton.gameObject.name);
        }
        else Debug.LogError("❌ dropButton NULL!");
        
        if (hotbarButton) 
        {
            hotbarButton.onClick.AddListener(OnHotbarClicked);
            Debug.Log("✅ OnHotbarClicked listener добавлен на " + hotbarButton.gameObject.name);
        }
        else Debug.LogError("❌ hotbarButton NULL!");

        // 🔍 ОТЛАДКА - проверяем все компоненты перед скрытием
        Debug.Log("═══ ItemContextMenu ОТЛАДКА ═══");
        Debug.Log($"menuPanel: {(menuPanel != null ? "✅ " + menuPanel.name : "❌ NULL")}");
        Debug.Log($"useButton: {(useButton != null ? "✅ " + useButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"dropButton: {(dropButton != null ? "✅ " + dropButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"hotbarButton: {(hotbarButton != null ? "✅ " + hotbarButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"canvasGroup: {(canvasGroup != null ? "✅" : "❌ NULL")}");
        
        if (canvasGroup != null)
        {
            Debug.Log($"canvasGroup начальное состояние:");
            Debug.Log($"  Alpha: {canvasGroup.alpha} (должно быть 0 изначально)");
            Debug.Log($"  Interactable: {canvasGroup.interactable} (должно быть false)");
            Debug.Log($"  Blocks Raycasts: {canvasGroup.blocksRaycasts} (должно быть false)");
        }

        HideMenu();
        Debug.Log("✅ ItemContextMenu инициализирован");
    }

    private void Update()
    {
        // Проверяем всё это только если меню сейчас видимо на экране
        if (canvasGroup.alpha > 0)
        {
            // Закрытие по клику ЛКМ "мимо" меню
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Получаем рамки нашего меню
                RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
                // Получаем текущие координаты мышки на экране
                Vector2 mousePos = Mouse.current.position.ReadValue();

                // Специальная функция Unity: она проверяет, попала ли мышка внутрь квадрата меню
                // Если НЕ попала (стоит знак !), то мы закрываем меню
                if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, null))
                {
                    HideMenu();
                }
            }
        }
    }

    public void ShowMenu(ItemData item, RectTransform slotRect)
    {
        if (item == null) return;
        selectedItem = item;

        RectTransform rect = menuPanel.GetComponent<RectTransform>();
        if (rect && slotRect != null)
        {
            // Меню появляется по центру слота с смещением
            rect.position = slotRect.position + new Vector3(50, -50, 0);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        // 🔴 Когда меню открывается - пусть hint panel тоже активна, если она нужна
        // (Но скрыта пока игрок не нажмет "Hotbar")
        
        Debug.Log($"📋 МЕНЮ ОТКРЫТО: {item.itemName}");
    }

    public void HideMenu()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        selectedItem = null;
        
        // 🔴 Закрываем hint panel
        HideHintPanel();
        
        // Отменяем режим выбора слота если он был
        if (HotbarManager.instance != null && HotbarManager.instance.IsPendingSlotSelection())
        {
            HotbarManager.instance.CancelSlotSelection();
        }
    }

    public bool IsOpen()
    {
        return canvasGroup != null && canvasGroup.alpha > 0.001f;
    }

    public void OnUseClicked()
    {
        if (selectedItem == null) return;

        Debug.Log($"✅ USE: {selectedItem.itemName}");

        // Если это хилка - используем и восстанавливаем здоровье
        if (selectedItem.itemType == ItemData.ItemType.HealthItem)
        {
            Debug.Log($"💊 Используем хилку: {selectedItem.itemName}");
            
            // Восстанавливаем здоровье
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(selectedItem.healAmount);
                Debug.Log($"🩹 +{selectedItem.healAmount} HP восстановлено");
            }
            
            // Удаляем предмет из инвентаря
            InventorySystemNew invSystem = FindFirstObjectByType<InventorySystemNew>();
            if (invSystem != null)
            {
                invSystem.RemoveItem(selectedItem.itemName, 1);
                Debug.Log($"✅ Хилка использована и удалена из инвентаря");
            }
            else
            {
                Debug.LogError("❌ InventorySystemNew НЕ НАЙДЕН!");
            }
        }
        // Если это оружие - экипируем (инвентарь закроется автоматически в EquipItemDirectly)
        else if (selectedItem.itemType == ItemData.ItemType.Weapon)
        {
            Debug.Log($"🔪 Берем оружие: {selectedItem.itemName}");
            EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
            if (em) em.EquipItemDirectly(selectedItem);
        }

        HideMenu();
    }

    public void OnDropClicked()
    {
        if (selectedItem == null) return;

        InventorySystemNew invSystem = FindFirstObjectByType<InventorySystemNew>();
        if (invSystem == null) 
        {
            Debug.LogError("❌ InventorySystemNew НЕ НАЙДЕН!");
            HideMenu();
            return;
        }

        // 🎯 НОВОЕ: Получаем ВСЕХ количество предметов в стопке
        int totalCount = invSystem.GetItemCount(selectedItem.itemName);
        
        Debug.Log($"🗑️ DROP ALL: {selectedItem.itemName} x{totalCount}");

        // Спавним ВСЕ предметы из стопки
        for (int i = 0; i < totalCount; i++)
        {
            SpawnDroppedItem(selectedItem);
        }

        // Удаляем ВСЮ стопку из инвентаря
        invSystem.RemoveItem(selectedItem.itemName, totalCount);

        // Если это оружие что в руках - разэкипируем
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
        if (em)
            em.OnItemDropped(selectedItem);

        // Удаляем предмет из хотбара, если он там был
        if (HotbarManager.instance != null)
        {
            HotbarManager.instance.RemoveItemFromHotbar(selectedItem);
        }

        HideMenu();
    }

    public void OnHotbarClicked()
    {
        if (selectedItem == null) return;

        Debug.Log($"🔥 HOTBAR: {selectedItem.itemName}");

        if (HotbarManager.instance == null)
        {
            Debug.LogError("❌ HotbarManager.instance NULL!");
            HideMenu();
            return;
        }

        // ✅ Показываем hint panel рядом с меню
        ShowHintPanel();

        // Переходим в режим выбора слота в HotbarManager
        HotbarManager.instance.SetPendingItemForHotbar(selectedItem);

        // 🔴 НЕ закрываем меню! Hint panel будет видна рядом
        // HideMenu();
    }
    
    private void ShowHintPanel()
    {
        if (hintPanel == null) return;

        hintPanel.SetActive(true);

        RectTransform hintRect = hintPanel.GetComponent<RectTransform>();
        RectTransform menuRect = menuPanel.GetComponent<RectTransform>();

        if (hintRect != null && menuRect != null)
        {
            // ✅ Якорь — левый нижний угол родителя (меню)
            hintRect.anchorMin = Vector2.zero;
            hintRect.anchorMax = Vector2.zero;
            // ✅ Pivot — левый центр хинт-панели
            hintRect.pivot = new Vector2(0f, 0.5f);

            // ✅ Ставим правее меню, по середине его высоты
            hintRect.anchoredPosition = new Vector2(
                menuRect.rect.width + hintPanelOffset.x,   // X: правый край меню + отступ
                menuRect.rect.height * 0.5f + hintPanelOffset.y  // Y: середина меню
            );
            
            Debug.Log($"✅ HintPanel позиция установлена: {hintRect.anchoredPosition}");
        }

        if (hintText != null)
        {
            hintText.text = "Выберите слот: 1  2  3  4 (Esc — отмена)";
        }
    }
    
    private void HideHintPanel()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);

        }
    }

    private void SpawnDroppedItem(ItemData itemData)
    {

        
        // Ищем камеру правильно
        Camera cam = FindFirstObjectByType<Camera>();
        if (!cam) 
        {

            return;
        }

        Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;



        GameObject template = FindObjectByName(itemData.itemName);
        
        if (!template) 
        {

            return;
        }



        GameObject drop = Instantiate(template, pos, Quaternion.identity);
        drop.name = itemData.itemName + " (Dropped)";
        

    }

    private GameObject FindObjectByName(string name)
    {

        
#pragma warning disable CS0618
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(name))
            {

                
                InteractableItem interactable = obj.GetComponent<InteractableItem>();
                if (interactable)
                {

                    return obj;
                }
                else
                {

                }
            }
        }
#pragma warning restore CS0618
        

        return null;
    }
}
