using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance;

    [Header("Small Slots")]
    public List<InventorySlot> smallSlots = new List<InventorySlot>();
    public int maxSmallSlots = 12;

    [Header("Weapon Slots")]
    public List<InventorySlot> weaponSlots = new List<InventorySlot>();
    public int maxWeaponSlots = 3;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged inventoryChanged;

    private bool inventoryUnlocked = false;

    public delegate void OnInventoryUnlocked();
    public event OnInventoryUnlocked inventoryUnlockedEvent;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        while (weaponSlots.Count < maxWeaponSlots)
            weaponSlots.Add(new InventorySlot(null, 0));
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData є null");
            return false;
        }

        if (itemData.itemType == ItemData.ItemType.Weapon)
            return AddItemToWeaponSlots(itemData, amount);
        else
            return AddItemToSmallSlots(itemData, amount);
    }

    public bool AddItemToSmallSlots(ItemData itemData, int amount = 1)
    {
        if (itemData == null || itemData.itemType == ItemData.ItemType.Weapon)
        {
            Debug.LogWarning("Зброю не можна додати в малі слоти");
            return false;
        }

        foreach (InventorySlot slot in smallSlots)
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

        if (smallSlots.Count < maxSmallSlots)
        {
            int addNow = Mathf.Min(amount, itemData.maxStackSize);
            smallSlots.Add(new InventorySlot(itemData, addNow));
            inventoryChanged?.Invoke();

            int remaining = amount - addNow;
            if (remaining > 0)
                return AddItemToSmallSlots(itemData, remaining);

            return true;
        }

        Debug.LogWarning("Малі слоти переповнені");
        return false;
    }

    public bool AddItemToWeaponSlots(ItemData weapon, int amount = 1)
    {
        if (weapon == null || weapon.itemType != ItemData.ItemType.Weapon)
        {
            Debug.LogWarning("Це не зброя");
            return false;
        }

        if (amount != 1) amount = 1;

        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Pistol)
        {
            while (weaponSlots.Count < 1)
                weaponSlots.Add(new InventorySlot(null, 0));

            if (weaponSlots[0].itemData == null || weaponSlots[0].count == 0)
            {
                weaponSlots[0] = new InventorySlot(weapon, 1);
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"Слот 0 для пістолета зайнятий: {weaponSlots[0].itemData.itemName}");
                return false;
            }
        }

        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Knife)
        {
            while (weaponSlots.Count < 2)
                weaponSlots.Add(new InventorySlot(null, 0));

            if (weaponSlots[1].itemData == null || weaponSlots[1].count == 0)
            {
                weaponSlots[1] = new InventorySlot(weapon, 1);
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"Слот 1 для ножа зайнятий: {weaponSlots[1].itemData.itemName}");
                return false;
            }
        }

        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Shotgun)
        {
            while (weaponSlots.Count < 3)
                weaponSlots.Add(new InventorySlot(null, 0));

            if (weaponSlots[2].itemData == null || weaponSlots[2].count == 0)
            {
                weaponSlots[2] = new InventorySlot(weapon, 1);
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning("Слот 2 для дробовика зайнятий");
                return false;
            }
        }

        if (weapon.weaponSlotType == ItemData.WeaponSlotType.General)
        {
            for (int i = 0; i < 2; i++)
            {
                while (weaponSlots.Count <= i)
                    weaponSlots.Add(new InventorySlot(null, 0));

                if (weaponSlots[i].itemData == null || weaponSlots[i].count == 0)
                {
                    weaponSlots[i] = new InventorySlot(weapon, 1);
                    inventoryChanged?.Invoke();
                    return true;
                }
            }
            Debug.LogWarning("Обидва слоти зброї зайняті");
            return false;
        }

        Debug.LogWarning($"Невідомий тип зброї: {weapon.weaponSlotType}");
        return false;
    }

    public bool RemoveItemFromSmallSlots(string itemName, int amount = 1)
    {
        for (int i = smallSlots.Count - 1; i >= 0; i--)
        {
            if (smallSlots[i].itemData != null && smallSlots[i].itemData.itemName == itemName)
            {
                if (smallSlots[i].count >= amount)
                {
                    smallSlots[i].count -= amount;
                    if (smallSlots[i].count == 0)
                        smallSlots.RemoveAt(i);

                    inventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

    public bool RemoveItemFromWeaponSlots(string weaponName)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].itemData != null && weaponSlots[i].itemData.itemName == weaponName)
            {
                weaponSlots[i] = new InventorySlot(null, 0);
                inventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public int GetItemCountSmallSlots(string itemName)
    {
        int total = 0;
        foreach (InventorySlot slot in smallSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
                total += slot.count;
        }
        return total;
    }

    public bool HasWeapon(string weaponName)
    {
        foreach (InventorySlot slot in weaponSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == weaponName)
                return true;
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int count = GetItemCountSmallSlots(itemName);

        foreach (InventorySlot slot in weaponSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
                count += slot.count;
        }

        return count;
    }

    public bool RemoveItem(string itemName, int amount = 1)
    {
        for (int i = smallSlots.Count - 1; i >= 0; i--)
        {
            if (smallSlots[i].itemData != null && smallSlots[i].itemData.itemName == itemName)
            {
                if (smallSlots[i].count >= amount)
                {
                    smallSlots[i].count -= amount;
                    if (smallSlots[i].count == 0)
                        smallSlots.RemoveAt(i);

                    inventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        return RemoveItemFromWeaponSlots(itemName);
    }

    public void UnlockInventory()
    {
        if (inventoryUnlocked) return;
        inventoryUnlocked = true;
        Debug.Log("Інвентар розблоковано");
        inventoryUnlockedEvent?.Invoke();
    }

    public bool IsInventoryUnlocked() => inventoryUnlocked;
}