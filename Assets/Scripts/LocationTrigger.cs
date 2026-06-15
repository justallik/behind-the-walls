using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Save ID")]
    [SerializeField] private string uniqueId; 

    [Header("Quest Requirement")]
    [SerializeField] private string requiredQuestCompleted;

    [Header("Quest On Enter")]
    [SerializeField] private string enterQuestToComplete;
    [SerializeField] private string enterQuestToActivate;
    [SerializeField] private string enterQuestToIncrement;

    [Header("Quest On Exit")]
    [SerializeField] private string exitQuestToComplete;
    [SerializeField] private string exitQuestToActivate;
    [SerializeField] private string exitQuestToIncrement;

    [Header("Inventory Check On Exit")]
    [SerializeField] private bool checkInventoryOnExit = false;
    [SerializeField] private string inventoryWeaponId;
    [SerializeField] private bool checkDiary = false;
    [SerializeField] private string inventoryQuestToComplete;
    [SerializeField] private string inventoryQuestToActivate;

    [Header("Zombie Encounter")]
    [SerializeField] private GameObject zombieObject;
    [SerializeField] private GameObject[] arenaWalls;
    [SerializeField] private bool spawnZombieOnEnter = false;
    [SerializeField] private bool spawnZombieOnExit = false;
    [SerializeField] private bool blockSprintDuringEncounter = false;

    private bool hasTriggered = false;
    private bool zombieTriggered = false;
    private bool zombieDied = false;
    private PlayerMovement playerMovement;
    private EnemyAI enemyAI;

    private void Start()
    {
        if (zombieObject != null)
        {
            zombieObject.SetActive(false);
            enemyAI = zombieObject.GetComponent<EnemyAI>();
        }

        SetArenaWalls(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerMovement = playerObj.GetComponent<PlayerMovement>();
    }

    public void OnLoadSave()
    {
        if (string.IsNullOrEmpty(uniqueId)) return;
        if (!SaveSystem.instance.IsTriggered(uniqueId)) return;

        hasTriggered = true;
        zombieTriggered = true;

        SetArenaWalls(false);
        if (zombieObject != null)
            zombieObject.SetActive(false);
    }

    private void Update()
    {
        if (zombieObject != null && zombieObject.activeSelf && enemyAI != null)
        {
            if (!zombieDied && enemyAI.currentState == EnemyAI.EnemyState.Die)
            {
                zombieDied = true;
                OnZombieDied();
            }
        }
    }

    private bool IsRequiredQuestDone()
    {
        if (string.IsNullOrEmpty(requiredQuestCompleted)) return true;
        if (QuestManager.instance == null) return false;
        return QuestManager.instance.IsQuestCompleted(requiredQuestCompleted);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsRequiredQuestDone()) return;

        if (spawnZombieOnEnter && zombieTriggered) return;

        if (spawnZombieOnEnter && !zombieTriggered)
        {
            zombieTriggered = true;
            RegisterTriggered();

            if (zombieObject != null)
                zombieObject.SetActive(true);

            SetArenaWalls(true);

            if (blockSprintDuringEncounter && playerMovement != null)
                playerMovement.TriggerExhaustion();

            TriggerQuestEvent(enterQuestToComplete, enterQuestToActivate, enterQuestToIncrement);
            return;
        }

        if (hasTriggered) return;
        if (spawnZombieOnExit) return;

        hasTriggered = true;
        RegisterTriggered();
        TriggerQuestEvent(enterQuestToComplete, enterQuestToActivate, enterQuestToIncrement);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spawnZombieOnExit && zombieTriggered) return;

        if (spawnZombieOnExit && !zombieTriggered)
        {
            if (!IsRequiredQuestDone()) return;

            zombieTriggered = true;
            RegisterTriggered();

            if (zombieObject != null)
                zombieObject.SetActive(true);

            SetArenaWalls(true);

            if (blockSprintDuringEncounter && playerMovement != null)
                playerMovement.TriggerExhaustion();

            TriggerQuestEvent(exitQuestToComplete, exitQuestToActivate, exitQuestToIncrement);
            return;
        }

        if (checkInventoryOnExit)
        {
            bool hasWeapon = string.IsNullOrEmpty(inventoryWeaponId) ||
                             InventorySystem.instance.HasWeapon(inventoryWeaponId);
            bool hasDiary = !checkDiary || DiaryManager.instance.IsDiaryUnlocked();

            if (hasWeapon && hasDiary)
                TriggerQuestEvent(inventoryQuestToComplete, inventoryQuestToActivate, null);
            else
                Debug.Log("Інвентар неповний — квест не виконано");

            return;
        }

        TriggerQuestEvent(exitQuestToComplete, exitQuestToActivate, exitQuestToIncrement);
    }

    private void OnZombieDied()
    {
        SetArenaWalls(false);

        if (blockSprintDuringEncounter && playerMovement != null)
            playerMovement.UnlockSprint();

        Debug.Log("Зомбі переможено — арена відкрита");
    }

    private void SetArenaWalls(bool active)
    {
        if (arenaWalls == null) return;
        foreach (GameObject wall in arenaWalls)
            if (wall != null)
                wall.SetActive(active);
    }

    private void RegisterTriggered()
    {
        if (!string.IsNullOrEmpty(uniqueId))
            SaveSystem.instance?.RegisterTriggered(uniqueId);
    }

    private void TriggerQuestEvent(string toComplete, string toActivate, string toIncrement)
    {
        if (QuestManager.instance == null) return;

        if (!string.IsNullOrEmpty(toComplete))
            QuestManager.instance.CompleteQuest(toComplete);

        if (!string.IsNullOrEmpty(toActivate))
            QuestManager.instance.ActivateQuest(toActivate);

        if (!string.IsNullOrEmpty(toIncrement))
            QuestManager.instance.IncrementQuestCounter(toIncrement);
    }
}