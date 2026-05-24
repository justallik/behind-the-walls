using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Inventory")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int maxSlots = 20;

    private bool inventoryUnlocked = false;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged inventoryChanged;

    public delegate void OnInventoryUnlocked();
    public event OnInventoryUnlocked inventoryUnlockedEvent;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData є null");
            return false;
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemData.itemName)
            {
                int canAdd = slot.GetRemainingSpace();
                if (canAdd >= amount)
                {
                    slot.count += amount;
                    inventoryChanged?.Invoke();
                    return true;
                }
                else if (canAdd > 0)
                {
                    slot.count += canAdd;
                    amount -= canAdd;
                }
            }
        }

        if (inventorySlots.Count < maxSlots)
        {
            int addNow = Mathf.Min(amount, itemData.maxStackSize);
            InventorySlot newSlot = new InventorySlot(itemData, addNow);
            inventorySlots.Add(newSlot);
            inventoryChanged?.Invoke();

            int remaining = amount - addNow;
            if (remaining > 0)
                return AddItem(itemData, remaining);

            return true;
        }

        Debug.LogWarning("Інвентар переповнений");
        return false;
    }

    public bool RemoveItem(string itemName, int amount = 1)
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i].itemData != null && inventorySlots[i].itemData.itemName == itemName)
            {
                if (inventorySlots[i].count >= amount)
                {
                    inventorySlots[i].count -= amount;
                    if (inventorySlots[i].count == 0)
                        inventorySlots.RemoveAt(i);

                    inventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int total = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
                total += slot.count;
        }
        return total;
    }

    public List<InventorySlot> GetAllSlots() => inventorySlots;

    public void UnlockInventory()
    {
        if (inventoryUnlocked) return;
        inventoryUnlocked = true;
        Debug.Log("Інвентар розблоковано");
        inventoryUnlockedEvent?.Invoke();
    }

    public bool IsInventoryUnlocked() => inventoryUnlocked;
}