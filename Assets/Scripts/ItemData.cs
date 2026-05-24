using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemData", fileName = "New Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Предмет";

    public enum ItemType { Weapon, Note, HealthItem, Ammunition, Diary, Backpack }
    public ItemType itemType;

    [Header("Diary Settings")]
    [Tooltip("ID запису від 1 до 14")]
    public int diaryEntryID;

    public enum WeaponSlotType { General, Pistol, Knife, Shotgun }

    [Header("Weapon Settings")]
    public WeaponSlotType weaponSlotType = WeaponSlotType.General;
    public float weaponDamage = 25f;
    public float attackStaminaCost = 15f;
    public float blockStaminaCost = 5f;
    public float blockReduction = 0.7f;

    [Header("Item Settings")]
    public int maxStackSize = 1;
    public int healAmount = 0;
    public Sprite itemIcon;
    public Sprite hotbarIcon;
}

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int count = 0;

    public InventorySlot(ItemData data, int amount = 1)
    {
        itemData = data;
        count = amount;
    }

    public bool CanAddMore()
    {
        return itemData != null && count < itemData.maxStackSize;
    }

    public int GetRemainingSpace()
    {
        return itemData != null ? itemData.maxStackSize - count : 0;
    }
}