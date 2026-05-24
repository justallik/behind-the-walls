using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public float posX, posY, posZ;
    public float rotY;
    public float health;
    public int lives;
    public float stamina;

    public bool inventoryUnlocked;
    public List<SavedItem> smallSlots = new List<SavedItem>();
    public List<SavedItem> weaponSlots = new List<SavedItem>();

    public List<string> activeQuests = new List<string>();
    public List<string> completedQuests = new List<string>();

    public bool diaryUnlocked;
    public List<int> diaryEntryIDs = new List<int>();
}

[System.Serializable]
public class SavedItem
{
    public string itemName;
    public int count;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;

    private string savePath => Application.persistentDataPath + "/save.json";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene" && SaveExists())
        {
            Invoke(nameof(DelayedLoad), 0.3f);
        }
    }

    private void DelayedLoad()
    {
        FindReferences();
        Load();
        Invoke(nameof(HideFadeAfterLoad), 0.5f);
    }

    private void HideFadeAfterLoad()
    {
        if (SleepSystem.instance != null)
            SleepSystem.instance.HideFade();

        CanvasGroup[] canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (CanvasGroup cg in canvasGroups)
        {
            string name = cg.gameObject.name.ToLower();
            if (name.Contains("fade") || name.Contains("blur") ||
                name.Contains("eyelid") || name.Contains("overlay"))
            {
                cg.alpha = 0f;
                cg.gameObject.SetActive(false);
            }
        }
    }

    private void FindReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("Гравця не знайдено при завантаженні");
        }
    }

    public void Save()
    {
        SaveData data = new SaveData();

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
            data.stamina = playerMovement.GetCurrentStamina();

        if (InventorySystem.instance != null)
        {
            data.inventoryUnlocked = InventorySystem.instance.IsInventoryUnlocked();

            foreach (var slot in InventorySystem.instance.smallSlots)
            {
                if (slot.itemData != null)
                    data.smallSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });
            }

            foreach (var slot in InventorySystem.instance.weaponSlots)
            {
                if (slot.itemData != null)
                    data.weaponSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });
            }
        }

        if (QuestManager.instance != null)
        {
            foreach (var quest in QuestManager.instance.GetAllQuests())
            {
                if (quest.isActive) data.activeQuests.Add(quest.questId);
                if (quest.isCompleted) data.completedQuests.Add(quest.questId);
            }
        }

        if (DiaryManager.instance != null)
        {
            data.diaryUnlocked = DiaryManager.instance.IsDiaryUnlocked();
            foreach (var entry in DiaryManager.instance.GetSortedEntries())
                data.diaryEntryIDs.Add(entry.id);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Гру збережено");
    }

    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Файл збереження не знайдено");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

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

        if (InventorySystem.instance != null)
        {
            if (data.inventoryUnlocked)
                InventorySystem.instance.UnlockInventory();

            foreach (var saved in data.smallSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null)
                    InventorySystem.instance.AddItemToSmallSlots(item, saved.count);
                else
                    Debug.LogWarning($"Предмет не знайдено: {saved.itemName}");
            }

            foreach (var saved in data.weaponSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null)
                    InventorySystem.instance.AddItemToWeaponSlots(item, saved.count);
                else
                    Debug.LogWarning($"Предмет не знайдено: {saved.itemName}");
            }
        }

        if (QuestManager.instance != null)
        {
            foreach (string id in data.activeQuests)
                QuestManager.instance.ActivateQuest(id);
            foreach (string id in data.completedQuests)
                QuestManager.instance.CompleteQuest(id);
        }

        if (DiaryManager.instance != null)
        {
            if (data.diaryUnlocked) DiaryManager.instance.UnlockDiary();
            foreach (int id in data.diaryEntryIDs)
                DiaryManager.instance.AddEntryByID(id);
        }

        Debug.Log("Гру завантажено");
    }

    public bool SaveExists() => File.Exists(savePath);

    public void NewGame()
    {
        DeleteSave();
        SceneManager.LoadScene("IntroQuote");
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Збереження видалено");
        }
    }

    public void ContinueGame()
    {
        if (SaveExists())
            SceneManager.LoadScene("SampleScene");
    }
}