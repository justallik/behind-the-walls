using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData myItem;
    private TextMeshProUGUI itemCountText;
    private bool isHovered = false;

    public void SetupSlot(ItemData item)
    {
        myItem = item;
        itemCountText = transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void Update()
    {
        if (myItem == null) return;

        if (isHovered && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (ItemContextMenu.instance != null)
                ItemContextMenu.instance.ShowMenu(myItem, GetComponent<RectTransform>());
            else
                Debug.LogError("ItemContextMenu не знайдено");
        }

        if (isHovered && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            EquipmentManager equipManager = FindFirstObjectByType<EquipmentManager>();
            if (equipManager != null)
                equipManager.EquipItemDirectly(myItem);
        }
    }

    private void AssignToHotbar(int slotIndex)
    {
        if (HotbarManager.instance != null)
            HotbarManager.instance.AssignItem(slotIndex, myItem);
    }
}