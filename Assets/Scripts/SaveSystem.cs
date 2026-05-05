using UnityEngine;
using System.Collections.Generic;
using System.IO;

// ==================== ДАННЫЕ СОХРАНЕНИЯ ====================
[System.Serializable]
public class SaveData
{
    // Игрок
    public float posX, posY, posZ;
    public float rotY;
    public float health;
    public int lives;
    public float stamina;

    // Инвентарь
    public bool inventoryUnlocked;
    public List<SavedItem> smallSlots = new List<SavedItem>();
    public List<SavedItem> weaponSlots = new List<SavedItem>();

    // Квесты
    public List<string> activeQuests = new List<string>();
    public List<string> completedQuests = new List<string>();

    // Дневник
    public bool diaryUnlocked;
    public List<int> diaryEntryIDs = new List<int>();
}

[System.Serializable]
public class SavedItem
{
    public string itemName;
    public int count;
}

// ==================== МЕНЕДЖЕР СОХРАНЕНИЙ ====================
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;

    private string savePath => Application.persistentDataPath + "/save.json";

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ==================== СОХРАНЕНИЕ ====================
    public void Save()
    {
        SaveData data = new SaveData();

        // --- Игрок ---
        if (playerTransform != null)
        {
            data.posX = playerTransform.position.x;
            data.posY = playerTransform.position.y;
            data.posZ = playerTransform.position.z;
            data.rotY = playerTransform.eulerAngles.y;
        }

        if (playerHealth != null)
        {
            data.health = playerHealth.currentHealth;
            data.lives = playerHealth.currentLives;
        }

        if (playerMovement != null)
        {
            data.stamina = playerMovement.GetCurrentStamina();
        }

        // --- Инвентарь ---
        if (InventorySystemNew.instance != null)
        {
            data.inventoryUnlocked = InventorySystemNew.instance.IsInventoryUnlocked();

            foreach (var slot in InventorySystemNew.instance.smallSlots)
            {
                if (slot.itemData != null)
                    data.smallSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });
            }

            foreach (var slot in InventorySystemNew.instance.weaponSlots)
            {
                if (slot.itemData != null)
                    data.weaponSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });
            }
        }

        // --- Квесты ---
        if (QuestManager.instance != null)
        {
            foreach (var quest in QuestManager.instance.GetAllQuests())
            {
                if (quest.isActive) data.activeQuests.Add(quest.questId);
                if (quest.isCompleted) data.completedQuests.Add(quest.questId);
            }
        }

        // --- Дневник ---
        if (DiaryManager.instance != null)
        {
            data.diaryUnlocked = DiaryManager.instance.IsDiaryUnlocked();
            foreach (var entry in DiaryManager.instance.GetSortedEntries())
                data.diaryEntryIDs.Add(entry.id);
        }

        // --- Запись в файл ---
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"✅ Игра сохранена: {savePath}");
    }

    // ==================== ЗАГРУЗКА ====================
    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("⚠️ Файл сохранения не найден!");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // --- Игрок ---
        if (playerTransform != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerTransform.position = new Vector3(data.posX, data.posY, data.posZ);
            playerTransform.eulerAngles = new Vector3(0, data.rotY, 0);
            if (cc != null) cc.enabled = true;
        }

        if (playerHealth != null)
        {
            playerHealth.currentHealth = data.health;
            playerHealth.currentLives = data.lives;
        }

        // --- Инвентарь ---
        if (InventorySystemNew.instance != null)
        {
            if (data.inventoryUnlocked)
                InventorySystemNew.instance.UnlockInventory();

            // Загружаем предметы из Resources
            foreach (var saved in data.smallSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null) InventorySystemNew.instance.AddItemToSmallSlots(item, saved.count);
                else Debug.LogWarning($"⚠️ Предмет не найден: {saved.itemName}");
            }

            foreach (var saved in data.weaponSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null) InventorySystemNew.instance.AddItemToWeaponSlots(item, saved.count);
                else Debug.LogWarning($"⚠️ Предмет не найден: {saved.itemName}");
            }
        }

        // --- Квесты ---
        if (QuestManager.instance != null)
        {
            foreach (string id in data.activeQuests)
                QuestManager.instance.ActivateQuest(id);
            foreach (string id in data.completedQuests)
                QuestManager.instance.CompleteQuest(id);
        }

        // --- Дневник ---
        if (DiaryManager.instance != null)
        {
            if (data.diaryUnlocked) DiaryManager.instance.UnlockDiary();
            foreach (int id in data.diaryEntryIDs)
                DiaryManager.instance.AddEntryByID(id);
        }

        Debug.Log("✅ Игра загружена!");
    }

    public bool SaveExists() => File.Exists(savePath);
}
