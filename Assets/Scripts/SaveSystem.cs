using UnityEngine;
using System.Collections;
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
    public List<SavedItem> smallSlots  = new List<SavedItem>();
    public List<SavedItem> weaponSlots = new List<SavedItem>();

    public List<string> activeQuests    = new List<string>();
    public List<string> completedQuests = new List<string>();

    public bool diaryUnlocked;
    public List<int> diaryEntryIDs = new List<int>();

    public List<string> pickedUpItems = new List<string>();
    public List<string> triggeredIds  = new List<string>();
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
    [SerializeField] private Transform      playerTransform;
    [SerializeField] private PlayerHealth   playerHealth;
    [SerializeField] private PlayerMovement playerMovement;

    private HashSet<string> _pickedItems  = new HashSet<string>();
    private HashSet<string> _triggeredIds = new HashSet<string>();

    private string savePath => Application.persistentDataPath + "/save.json";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene" && SaveExists())
            StartCoroutine(WaitAndLoad());
    }

    // ── Завантаження після переходу в сцену ──────────────────────────────────

    private IEnumerator WaitAndLoad()
    {
        // Чекаємо поки всі ключові системи будуть готові
        yield return new WaitUntil(() =>
            QuestManager.instance    != null &&
            InventorySystem.instance != null &&
            DiaryManager.instance    != null &&
            PlayerHealth.instance    != null
        );

        // Ще один кадр — щоб усі Start() виконались
        yield return null;

        FindReferences();
        Load();

        // Чекаємо поки SetPositionDelayed відпрацює
        yield return new WaitForSeconds(0.5f);

        HideFadeAfterLoad();

        // Чекаємо ще кадр щоб QuestUI.Start() точно виконався
        yield return null;

        QuestUI questUI = FindFirstObjectByType<QuestUI>(FindObjectsInactive.Include);
        if (questUI != null)
            questUI.RefreshAfterLoad();
    }

    private void HideFadeAfterLoad()
    {
        if (SleepSystem.instance != null)
            SleepSystem.instance.HideFade();

        CanvasGroup[] canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (CanvasGroup cg in canvasGroups)
        {
            string n = cg.gameObject.name.ToLower();
            if (n.Contains("fade") || n.Contains("blur") ||
                n.Contains("eyelid") || n.Contains("overlay"))
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
            playerHealth    = playerObj.GetComponent<PlayerHealth>();
            playerMovement  = playerObj.GetComponent<PlayerMovement>();

            if (playerHealth == null)
                Debug.LogError("SaveSystem: PlayerHealth не знайдено на гравці");
            if (playerMovement == null)
                Debug.LogError("SaveSystem: PlayerMovement не знайдено на гравці");
        }
        else
        {
            Debug.LogError("SaveSystem: гравця з тегом Player не знайдено");
        }
    }

    // ── Телепортація з затримкою ──────────────────────────────────────────────

    private IEnumerator SetPositionDelayed(Vector3 position, float rotY)
    {
        // Чекаємо два фізичних кадри — щоб всі коліжени сцени прорахувались
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (playerTransform == null) yield break;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position    = position;
        playerTransform.eulerAngles = new Vector3(0, rotY, 0);

        // Ще один кадр після телепортації перед вмиканням CC
        yield return new WaitForFixedUpdate();

        if (cc != null) cc.enabled = true;

        Debug.Log($"SaveSystem: позицію відновлено: {position}");
    }

    // ── Предмети ──────────────────────────────────────────────────────────────

    public void RegisterPickedItem(string id) => _pickedItems.Add(id);
    public bool IsItemPickedUp(string id)     => _pickedItems.Contains(id);

    // ── Тригери ───────────────────────────────────────────────────────────────

    public void RegisterTriggered(string id)
    {
        if (!string.IsNullOrEmpty(id)) _triggeredIds.Add(id);
    }

    public bool IsTriggered(string id)
    {
        return !string.IsNullOrEmpty(id) && _triggeredIds.Contains(id);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    public void Save()
    {
        SaveData data = new SaveData();

        // Позиція
        if (playerTransform != null)
        {
            data.posX = playerTransform.position.x;
            data.posY = playerTransform.position.y;
            data.posZ = playerTransform.position.z;
            data.rotY = playerTransform.eulerAngles.y;
        }

        // Здоров'я та життя
        if (playerHealth != null)
        {
            data.health = playerHealth.currentHealth;
            data.lives  = playerHealth.currentLives;
        }

        // Стаміна
        if (playerMovement != null)
            data.stamina = playerMovement.GetCurrentStamina();

        // Інвентар
        if (InventorySystem.instance != null)
        {
            data.inventoryUnlocked = InventorySystem.instance.IsInventoryUnlocked();

            foreach (var slot in InventorySystem.instance.smallSlots)
                if (slot.itemData != null)
                    data.smallSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });

            foreach (var slot in InventorySystem.instance.weaponSlots)
                if (slot.itemData != null)
                    data.weaponSlots.Add(new SavedItem { itemName = slot.itemData.itemName, count = slot.count });
        }

        // Квести
        if (QuestManager.instance != null)
        {
            foreach (var quest in QuestManager.instance.GetAllQuests())
            {
                if (quest.isCompleted)
                    data.completedQuests.Add(quest.questId);
                else if (quest.isActive)
                    data.activeQuests.Add(quest.questId);
            }
        }

        // Щоденник
        if (DiaryManager.instance != null)
        {
            data.diaryUnlocked = DiaryManager.instance.IsDiaryUnlocked();
            foreach (var entry in DiaryManager.instance.GetSortedEntries())
                data.diaryEntryIDs.Add(entry.id);
        }

        // Тригери та предмети
        data.pickedUpItems = new List<string>(_pickedItems);
        data.triggeredIds  = new List<string>(_triggeredIds);

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("Гру збережено");
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("SaveSystem: файл збереження не знайдено");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        if (data == null)
        {
            Debug.LogError("SaveSystem: не вдалось розпарсити файл збереження");
            return;
        }

        // Предмети
        _pickedItems = new HashSet<string>(data.pickedUpItems ?? new List<string>());
        foreach (var item in FindObjectsByType<InteractableItem>(FindObjectsSortMode.None))
            if (!string.IsNullOrEmpty(item.uniqueId) && _pickedItems.Contains(item.uniqueId))
                item.gameObject.SetActive(false);

        // Тригери
        _triggeredIds = new HashSet<string>(data.triggeredIds ?? new List<string>());
        foreach (var lt in FindObjectsByType<LocationTrigger>(FindObjectsSortMode.None))
            lt.OnLoadSave();
        foreach (var it in FindObjectsByType<InteractableTrigger>(FindObjectsSortMode.None))
            it.OnLoadSave();
        foreach (var dt in FindObjectsByType<DialogueTrigger>(FindObjectsSortMode.None))
            dt.OnLoadSave();

        // Позиція — через корутин з затримкою щоб уникнути провалювання крізь геометрію
        StartCoroutine(SetPositionDelayed(
            new Vector3(data.posX, data.posY, data.posZ), data.rotY));

        // Здоров'я та життя
        if (playerHealth != null)
        {
            playerHealth.currentHealth = data.health;
            playerHealth.currentLives  = data.lives;
        }

        // Стаміна
        if (playerMovement != null && data.stamina > 0)
            playerMovement.SetStamina(data.stamina);

        // Інвентар
        if (InventorySystem.instance != null)
        {
            if (data.inventoryUnlocked)
                InventorySystem.instance.UnlockInventory();

            foreach (var saved in data.smallSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null) InventorySystem.instance.AddItemToSmallSlots(item, saved.count);
                else Debug.LogWarning($"SaveSystem: предмет не знайдено: {saved.itemName}");
            }

            foreach (var saved in data.weaponSlots)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{saved.itemName}");
                if (item != null) InventorySystem.instance.AddItemToWeaponSlots(item, saved.count);
                else Debug.LogWarning($"SaveSystem: зброю не знайдено: {saved.itemName}");
            }
        }

        // Квести — silent щоб не спрацьовували тригери повторно
        if (QuestManager.instance != null)
        {
            foreach (string id in data.completedQuests)
                QuestManager.instance.CompleteQuestSilent(id);
            foreach (string id in data.activeQuests)
                QuestManager.instance.ActivateQuestSilent(id);
        }

        // Щоденник
        if (DiaryManager.instance != null)
        {
            if (data.diaryUnlocked) DiaryManager.instance.UnlockDiary();
            foreach (int id in data.diaryEntryIDs)
                DiaryManager.instance.AddEntryByID(id);
        }

        Debug.Log("Гру завантажено");
    }

    // ── Утиліти ───────────────────────────────────────────────────────────────

    public bool SaveExists() => File.Exists(savePath);

    public void NewGame()
    {
        _pickedItems.Clear();
        _triggeredIds.Clear();
        DeleteSave();
        SceneManager.LoadScene("IntroQuote");
    }

    public void ContinueGame()
    {
        if (SaveExists())
            SceneManager.LoadScene("SampleScene");
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("SaveSystem: збереження видалено");
        }
    }
}