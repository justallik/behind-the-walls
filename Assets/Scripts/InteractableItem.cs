using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Save")]
    public string uniqueId;

    [Header("Item Settings")]
    public ItemData itemData;

    [Header("Quest")]
    public string questIdToComplete;
    public string questIdToActivate;
    public string questIdToIncrement;

    [Header("Hint")]
    [SerializeField] private string hintAfterPickup;
    [SerializeField] private float hintDuration = 3f;

    [Header("Note")]
    public bool playCutsceneOnClose = false;

    private void Start()
    {
        if (!string.IsNullOrEmpty(uniqueId) && SaveSystem.instance != null)
        {
            if (SaveSystem.instance.IsItemPickedUp(uniqueId))
            {
                gameObject.SetActive(false);
                return;
            }
        }

        if (itemData == null) return;
        if (itemData.itemType != ItemData.ItemType.Note) return;

        if (DiaryManager.instance != null)
        {
            if (!DiaryManager.instance.IsDiaryUnlocked())
            {
                gameObject.SetActive(false);
                DiaryManager.instance.diaryUnlockedEvent += OnDiaryUnlocked;
            }
        }
    }

    private void OnDestroy()
    {
        if (DiaryManager.instance != null && itemData != null && itemData.itemType == ItemData.ItemType.Note)
            DiaryManager.instance.diaryUnlockedEvent -= OnDiaryUnlocked;
    }

    private void OnDiaryUnlocked()
    {
        if (gameObject != null) gameObject.SetActive(true);
    }

    private void RegisterAndDestroy()
    {
        if (!string.IsNullOrEmpty(uniqueId) && SaveSystem.instance != null)
            SaveSystem.instance.RegisterPickedItem(uniqueId);

        if (!string.IsNullOrEmpty(hintAfterPickup))
            HintManager.instance?.ShowHint(hintAfterPickup, hintDuration);

        Destroy(gameObject);
    }

    public void Interact()
    {
        if (itemData == null) return;

        if (itemData.itemType == ItemData.ItemType.Diary)
        {
            if (DiaryManager.instance != null)
                DiaryManager.instance.UnlockDiary();
            else
                Debug.LogError("DiaryManager не знайдено");

            TryUpdateQuest();
            RegisterAndDestroy();
            return;
        }

        if (itemData.itemType == ItemData.ItemType.Backpack)
        {
            if (InventorySystem.instance != null)
                InventorySystem.instance.UnlockInventory();
            else
                Debug.LogError("InventorySystem не знайдено");

            TryUpdateQuest();
            RegisterAndDestroy();
            return;
        }

        if (InventorySystem.instance == null) return;

        if (itemData.itemType == ItemData.ItemType.Note)
        {
            if (DiaryManager.instance != null && DiaryManager.instance.IsDiaryUnlocked())
                DiaryManager.instance.AddEntryByID(itemData.diaryEntryID);

            if (NoteViewer.instance != null)
            {
                NoteViewer.instance.ShowNote(
                    itemData.diaryEntryID,
                    questIdToComplete,
                    questIdToActivate,
                    playCutsceneOnClose
                );
            }
            else
            {
                Debug.LogError("NoteViewer не знайдено");
                TryUpdateQuest();
            }

            RegisterAndDestroy();
            return;
        }

        bool success = InventorySystem.instance.AddItem(itemData, 1);
        if (success)
        {
            TryUpdateQuest();
            RegisterAndDestroy();
        }
    }

    private void TryUpdateQuest()
    {
        if (QuestManager.instance == null) return;

        if (!string.IsNullOrEmpty(questIdToIncrement))
            QuestManager.instance.IncrementQuestCounter(questIdToIncrement);

        if (!string.IsNullOrEmpty(questIdToComplete))
            QuestManager.instance.CompleteQuest(questIdToComplete);

        if (!string.IsNullOrEmpty(questIdToActivate))
            QuestManager.instance.ActivateQuest(questIdToActivate);

        if ((itemData.itemType == ItemData.ItemType.Weapon &&
            itemData.weaponSlotType == ItemData.WeaponSlotType.Knife) ||
            itemData.itemType == ItemData.ItemType.Diary)
        {
            QuestManager.instance.TryCompleteSearchHuts();
        }
    }
}