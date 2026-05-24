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

    [Header("Hint Panel")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Vector2 hintPanelOffset = new Vector2(5, 0);

    private ItemData selectedItem = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (menuPanel == null)
            menuPanel = gameObject;

        if (canvasGroup == null)
        {
            canvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = menuPanel.AddComponent<CanvasGroup>();
        }

        if (useButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("Use")) { useButton = b; break; }
            }
        }

        if (dropButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("Drop")) { dropButton = b; break; }
            }
        }

        if (hotbarButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("Hotbar")) { hotbarButton = b; break; }
            }
        }

        if (useButton != null) useButton.onClick.AddListener(OnUseClicked);
        else Debug.LogError("UseButton не знайдено");

        if (dropButton != null) dropButton.onClick.AddListener(OnDropClicked);
        else Debug.LogError("DropButton не знайдено");

        if (hotbarButton != null) hotbarButton.onClick.AddListener(OnHotbarClicked);
        else Debug.LogError("HotbarButton не знайдено");

        HideMenu();
    }

    private void Update()
    {
        if (canvasGroup.alpha > 0)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
                Vector2 mousePos = Mouse.current.position.ReadValue();

                if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, null))
                    HideMenu();
            }
        }
    }

    public void ShowMenu(ItemData item, RectTransform slotRect)
    {
        if (item == null) return;
        selectedItem = item;

        RectTransform rect = menuPanel.GetComponent<RectTransform>();
        if (rect && slotRect != null)
            rect.position = slotRect.position + new Vector3(50, -50, 0);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void HideMenu()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        selectedItem = null;

        HideHintPanel();

        if (HotbarManager.instance != null && HotbarManager.instance.IsPendingSlotSelection())
            HotbarManager.instance.CancelSlotSelection();
    }

    public bool IsOpen() => canvasGroup != null && canvasGroup.alpha > 0.001f;

    public void OnUseClicked()
    {
        if (selectedItem == null) return;

        if (selectedItem.itemType == ItemData.ItemType.HealthItem)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.Heal(selectedItem.healAmount);

            InventorySystem invSystem = FindFirstObjectByType<InventorySystem>();
            if (invSystem != null)
                invSystem.RemoveItem(selectedItem.itemName, 1);
            else
                Debug.LogError("InventorySystem не знайдено");
        }
        else if (selectedItem.itemType == ItemData.ItemType.Weapon)
        {
            EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
            if (em != null) em.EquipItemDirectly(selectedItem);
        }

        HideMenu();
    }

    public void OnDropClicked()
    {
        if (selectedItem == null) return;

        InventorySystem invSystem = FindFirstObjectByType<InventorySystem>();
        if (invSystem == null)
        {
            Debug.LogError("InventorySystem не знайдено");
            HideMenu();
            return;
        }

        int totalCount = invSystem.GetItemCount(selectedItem.itemName);

        for (int i = 0; i < totalCount; i++)
            SpawnDroppedItem(selectedItem);

        invSystem.RemoveItem(selectedItem.itemName, totalCount);

        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
        if (em != null) em.OnItemDropped(selectedItem);

        if (HotbarManager.instance != null)
            HotbarManager.instance.RemoveItemFromHotbar(selectedItem);

        HideMenu();
    }

    public void OnHotbarClicked()
    {
        if (selectedItem == null) return;

        if (HotbarManager.instance == null)
        {
            Debug.LogError("HotbarManager не знайдено");
            HideMenu();
            return;
        }

        ShowHintPanel();
        HotbarManager.instance.SetPendingItemForHotbar(selectedItem);
    }

    private void ShowHintPanel()
    {
        if (hintPanel == null) return;

        hintPanel.SetActive(true);

        RectTransform hintRect = hintPanel.GetComponent<RectTransform>();
        RectTransform menuRect = menuPanel.GetComponent<RectTransform>();

        if (hintRect != null && menuRect != null)
        {
            hintRect.anchorMin = Vector2.zero;
            hintRect.anchorMax = Vector2.zero;
            hintRect.pivot = new Vector2(0f, 0.5f);
            hintRect.anchoredPosition = new Vector2(
                menuRect.rect.width + hintPanelOffset.x,
                menuRect.rect.height * 0.5f + hintPanelOffset.y
            );
        }

        if (hintText != null)
            hintText.text = "Оберіть слот: 1  2  3  4 (Esc — скасувати)";
    }

    private void HideHintPanel()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void SpawnDroppedItem(ItemData itemData)
    {
        Camera cam = FindFirstObjectByType<Camera>();
        if (!cam) return;

        Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;

        GameObject template = FindObjectByName(itemData.itemName);
        if (!template) return;

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
                if (interactable) return obj;
            }
        }
#pragma warning restore CS0618
        return null;
    }
}