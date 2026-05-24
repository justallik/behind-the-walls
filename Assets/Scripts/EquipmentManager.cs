using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Weapon Holder")]
    public Transform weaponHolder;

    public ItemData currentEquippedItem = null;
    public bool isEquipped = false;

    private PlayerHealth cachedPlayerHealth;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        cachedPlayerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        bool isPendingSlotSelection = HotbarManager.instance != null && HotbarManager.instance.IsPendingSlotSelection();

        if (Cursor.lockState == CursorLockMode.None && !isPendingSlotSelection)
            return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) HandleSlotKey(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) HandleSlotKey(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) HandleSlotKey(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) HandleSlotKey(3);
        }

        if (!isPendingSlotSelection && Mouse.current != null)
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (scrollDelta > 0)
            {
                HotbarManager.instance.NextSlot();
                HandleScrollAction(HotbarManager.instance.GetCurrentSlotIndex());
            }
            else if (scrollDelta < 0)
            {
                HotbarManager.instance.PreviousSlot();
                HandleScrollAction(HotbarManager.instance.GetCurrentSlotIndex());
            }
        }

        // F — використати хілку або прибрати зброю
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isEquipped && currentEquippedItem != null && !IsItemInHotbar(currentEquippedItem))
            {
                isEquipped = false;
                UpdateWeaponVisibility();
                return;
            }

            if (!isPendingSlotSelection && HotbarManager.instance != null)
            {
                int activeIndex = HotbarManager.instance.GetCurrentSlotIndex();
                ItemData activeItem = HotbarManager.instance.boundItems[activeIndex];

                if (activeItem != null && activeItem.itemType != ItemData.ItemType.Weapon)
                    TryUseItem(activeIndex);
            }
        }
    }

    private bool IsItemInHotbar(ItemData item)
    {
        if (HotbarManager.instance == null) return false;
        for (int i = 0; i < 4; i++)
        {
            if (HotbarManager.instance.boundItems[i] != null &&
                HotbarManager.instance.boundItems[i].itemName == item.itemName)
                return true;
        }
        return false;
    }

    private void HandleScrollAction(int slotIndex)
    {
        ItemData item = HotbarManager.instance.boundItems[slotIndex];

        if (item != null && item.itemType == ItemData.ItemType.Weapon)
        {
            currentEquippedItem = item;
            isEquipped = true;
            UpdateWeaponVisibility();
        }
        else
        {
            isEquipped = false;
            UpdateWeaponVisibility();
        }
    }

    private void HandleSlotKey(int slotIndex)
    {
        if (HotbarManager.instance.IsPendingSlotSelection())
        {
            HotbarManager.instance.ConfirmSlotSelection(slotIndex);
            return;
        }

        HotbarManager.instance.SetCurrentSlot(slotIndex);

        ItemData item = HotbarManager.instance.boundItems[slotIndex];

        if (item == null)
        {
            isEquipped = false;
            UpdateWeaponVisibility();
            return;
        }
        else if (item.itemType == ItemData.ItemType.HealthItem)
        {
            isEquipped = false;
            UpdateWeaponVisibility();
        }

        TryUseItem(slotIndex);
    }

    private void TryUseItem(int slotIndex)
    {
        ItemData item = HotbarManager.instance.boundItems[slotIndex];
        if (item == null) return;

        if (item.itemType == ItemData.ItemType.HealthItem)
        {
            if (InventorySystem.instance.GetItemCount(item.itemName) > 0)
            {
                if (cachedPlayerHealth != null)
                    cachedPlayerHealth.Heal(item.healAmount);

                InventorySystem.instance.RemoveItem(item.itemName, 1);
            }
        }
        else if (item.itemType == ItemData.ItemType.Weapon)
        {
            if (currentEquippedItem == item) isEquipped = !isEquipped;
            else { currentEquippedItem = item; isEquipped = true; }
            UpdateWeaponVisibility();
        }
        else
        {
            if (InventorySystem.instance.GetItemCount(item.itemName) > 0)
                InventorySystem.instance.RemoveItem(item.itemName, 1);
        }
    }

    public void EquipItemDirectly(ItemData item)
    {
        if (item == null) return;

        if (item.itemType == ItemData.ItemType.Weapon)
        {
            currentEquippedItem = item;
            isEquipped = true;
            UpdateWeaponVisibility();
            if (InventoryUI.instance != null) InventoryUI.instance.CloseInventory();
        }
        else
        {
            InventorySystem.instance.RemoveItem(item.itemName, 1);
        }
    }

    public void UpdateWeaponVisibility()
    {
        if (weaponHolder == null) return;

        weaponHolder.gameObject.SetActive(isEquipped);

        if (isEquipped && currentEquippedItem != null)
        {
            foreach (Transform weapon in weaponHolder)
            {
                weapon.gameObject.SetActive(weapon.name == currentEquippedItem.itemName);
            }
        }
    }

    public void OnItemDropped(ItemData item)
    {
        if (item.itemType == ItemData.ItemType.Weapon && currentEquippedItem == item)
        {
            isEquipped = false;
            UpdateWeaponVisibility();
        }
    }

    public GameObject GetActiveWeaponObject()
    {
        if (!isEquipped || currentEquippedItem == null) return null;

        foreach (Transform weapon in weaponHolder)
        {
            if (weapon.gameObject.activeSelf)
                return weapon.gameObject;
        }
        return null;
    }
}