using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager instance;

    [Header("Slot Icons")]
    [Tooltip("Порядок: 0-Вгору(1), 1-Вправо(2), 2-Вниз(3), 3-Вліво(4)")]
    public Image[] slotIcons = new Image[4];

    [Header("Slot Highlights")]
    public Image[] slotHighlights = new Image[4];

    [Header("Bound Items")]
    public ItemData[] boundItems = new ItemData[4];

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip assignToHotbarClip;

    private int currentSlotIndex = 0;
    private bool isPendingSlotSelection = false;
    private ItemData pendingItem = null;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (slotIcons[0] == null)
        {
            Image[] allImages = GetComponentsInChildren<Image>();
            int count = 0;
            foreach (Image img in allImages)
            {
                if (count < 4 && img.gameObject.name.Contains("Slot"))
                {
                    slotIcons[count] = img;
                    count++;
                }
            }
        }

        if (slotHighlights[0] == null)
        {
            Image[] allImages = GetComponentsInChildren<Image>();
            int count = 0;
            foreach (Image img in allImages)
            {
                if (count < 4 && img.gameObject.name.Contains("Highlight"))
                {
                    slotHighlights[count] = img;
                    count++;
                }
            }
        }

        UpdateHotbarUI();

        if (InventorySystem.instance != null)
            InventorySystem.instance.inventoryChanged += UpdateHotbarUI;
    }

    private void Update()
    {
        if (!isPendingSlotSelection) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelSlotSelection();
        }
    }

    private void OnDestroy()
    {
        if (InventorySystem.instance != null)
            InventorySystem.instance.inventoryChanged -= UpdateHotbarUI;
    }

    public void AssignItem(int slotIndex, ItemData item)
    {
        if (item == null) return;

        int oldSlotIndex = -1;

        for (int i = 0; i < boundItems.Length; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                oldSlotIndex = i;
                break;
            }
        }

        ItemData itemInTargetSlot = boundItems[slotIndex];
        boundItems[slotIndex] = item;

        if (oldSlotIndex != -1)
            boundItems[oldSlotIndex] = itemInTargetSlot;

        UpdateHotbarUI();
    }

    public void UpdateHotbarUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] != null)
            {
                int itemCount = 0;
                if (InventorySystem.instance != null)
                    itemCount = InventorySystem.instance.GetItemCount(boundItems[i].itemName);

                if (itemCount > 0)
                {
                    slotIcons[i].sprite = boundItems[i].hotbarIcon != null
                        ? boundItems[i].hotbarIcon
                        : boundItems[i].itemIcon;

                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].enabled = false;
                    boundItems[i] = null;
                }
            }
            else
            {
                slotIcons[i].enabled = false;
            }

            if (slotHighlights[i] != null)
                slotHighlights[i].enabled = (i == currentSlotIndex);
        }
    }

    public void RemoveItemFromHotbar(ItemData item)
    {
        if (item == null) return;

        for (int i = 0; i < boundItems.Length; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                boundItems[i] = null;
                UpdateHotbarUI();
                return;
            }
        }
    }

    public bool AddItemToFirstFreeSlot(ItemData item)
    {
        if (item == null) return false;

        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                Debug.LogWarning($"{item.itemName} вже є в хотбарі");
                return false;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] == null)
            {
                AssignItem(i, item);
                return true;
            }
        }

        Debug.LogWarning("Хотбар повний");
        return false;
    }

    public void SetPendingItemForHotbar(ItemData item)
    {
        if (item == null) return;

        isPendingSlotSelection = true;
        pendingItem = item;

        Debug.Log($"Оберіть слот для {item.itemName} — натисніть 1, 2, 3 або 4");
    }

    public void ConfirmSlotSelection(int slotIndex)
    {
        if (!isPendingSlotSelection || pendingItem == null)
        {
            Debug.LogWarning("Не в режимі вибору слота");
            return;
        }

        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogWarning("Невірний індекс слота");
            return;
        }

        string itemName = pendingItem.itemName;

        AssignItem(slotIndex, pendingItem);

        if (ItemContextMenu.instance != null)
            ItemContextMenu.instance.HideMenu();

        if (uiAudioSource != null && assignToHotbarClip != null)
            uiAudioSource.PlayOneShot(assignToHotbarClip);

        isPendingSlotSelection = false;
        pendingItem = null;

        Debug.Log($"{itemName} додано в слот {slotIndex + 1}");
    }

    public bool IsPendingSlotSelection() => isPendingSlotSelection;

    public void CancelSlotSelection()
    {
        if (isPendingSlotSelection)
        {
            isPendingSlotSelection = false;
            pendingItem = null;
        }
    }

    public void NextSlot()
    {
        currentSlotIndex++;
        if (currentSlotIndex >= 4) currentSlotIndex = 0;
        UpdateHotbarUI();
    }

    public void PreviousSlot()
    {
        currentSlotIndex--;
        if (currentSlotIndex < 0) currentSlotIndex = 3;
        UpdateHotbarUI();
    }

    public int GetCurrentSlotIndex() => currentSlotIndex;

    public void SetCurrentSlot(int index)
    {
        if (index >= 0 && index < 4)
        {
            currentSlotIndex = index;
            UpdateHotbarUI();
        }
    }
}