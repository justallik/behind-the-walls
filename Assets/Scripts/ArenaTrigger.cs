using UnityEngine;

public class ArenaTrigger : MonoBehaviour
{
    [Header("Zombie")]
    [SerializeField] private GameObject zombieObject;

    [Header("Return Point")]
    [SerializeField] private Transform returnPoint;

    [Header("Respawn")]
    [SerializeField] private Transform arenaRespawnPoint;

    private EnemyAI enemyAI;
    private bool encounterActive = false;
    private Transform originalRespawnPoint;

    private void Start()
    {
        if (zombieObject != null)
            enemyAI = zombieObject.GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if (zombieObject != null && zombieObject.activeSelf && !encounterActive)
        {
            encounterActive = true;

            // Зберігаємо оригінальну точку і підміняємо на точку арени
            if (PlayerHealth.instance != null && arenaRespawnPoint != null)
            {
                originalRespawnPoint = PlayerHealth.instance.respawnPoint;
                PlayerHealth.instance.respawnPoint = arenaRespawnPoint;
            }
        }

        if (encounterActive && enemyAI != null && enemyAI.currentState == EnemyAI.EnemyState.Die)
        {
            encounterActive = false;

            // Повертаємо оригінальну точку респавну
            if (PlayerHealth.instance != null && originalRespawnPoint != null)
                PlayerHealth.instance.respawnPoint = originalRespawnPoint;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!encounterActive) return;

        Debug.Log("Не можна піти — зомбі ще живий");

        CharacterController cc = other.GetComponent<CharacterController>();
        Transform returnPos = returnPoint != null ? returnPoint : transform;

        if (cc != null)
        {
            cc.enabled = false;
            other.transform.position = returnPos.position;
            cc.enabled = true;
        }
        else
        {
            other.transform.position = returnPos.position;
        }
    }
}