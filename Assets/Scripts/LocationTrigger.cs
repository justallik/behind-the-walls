using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private string questIdToComplete;
    [SerializeField] private string questIdToActivate;
    [SerializeField] private string questIdToIncrement;

    [Header("Inventory Check")]
    [SerializeField] private bool checkInventoryOnExit = false;

    [Header("Zombie Encounter")]
    [SerializeField] private GameObject zombieObject;
    [SerializeField] private GameObject[] arenaWalls;
    [SerializeField] private bool spawnZombieOnExit = false;
    [SerializeField] private bool blockSprintDuringEncounter = false;

    private bool hasTriggered = false;
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

    private void OnTriggerEnter(Collider collision)
    {
        if (hasTriggered) return;
        if (!collision.CompareTag("Player")) return;
        if (spawnZombieOnExit) return;

        hasTriggered = true;
        TriggerQuestEvent();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spawnZombieOnExit && !hasTriggered)
        {
            hasTriggered = true;

            if (zombieObject != null)
                zombieObject.SetActive(true);

            SetArenaWalls(true);

            if (blockSprintDuringEncounter && playerMovement != null)
                playerMovement.TriggerExhaustion();

            TriggerQuestEvent();
            return;
        }

        if (!checkInventoryOnExit) return;

        bool hasKnife = InventorySystem.instance.HasWeapon("Knife");
        bool hasDiary = DiaryManager.instance.IsDiaryUnlocked();

        if (hasKnife && hasDiary)
        {
            QuestManager.instance.CompleteQuest("quest_leave_hut");
            QuestManager.instance.ActivateQuest("quest_survive");
        }
        else
        {
            Debug.Log("Немає ножа або щоденника");
        }
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
        {
            if (wall != null)
                wall.SetActive(active);
        }
    }

    private void TriggerQuestEvent()
    {
        if (QuestManager.instance == null) return;

        if (!string.IsNullOrEmpty(questIdToComplete))
            QuestManager.instance.CompleteQuest(questIdToComplete);

        if (!string.IsNullOrEmpty(questIdToActivate))
            QuestManager.instance.ActivateQuest(questIdToActivate);

        if (!string.IsNullOrEmpty(questIdToIncrement))
            QuestManager.instance.IncrementQuestCounter(questIdToIncrement);
    }
}