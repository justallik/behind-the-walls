using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Respawn")]
    public int currentLives = 10;
    public Transform respawnPoint;

    [Header("Combat")]
    public PlayerCombat combatScript;

    [Header("I-Frames")]
    [SerializeField] private float iFrameDuration = 0.5f;
    private float iFrameTimer = 0f;
    public bool isInvulnerable = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (combatScript == null) combatScript = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (isInvulnerable)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0)
                isInvulnerable = false;
        }
    }

    public void StartIFrames()
    {
        isInvulnerable = true;
        iFrameTimer = iFrameDuration;
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable) return;

        float finalDamage = amount;

        if (combatScript != null && combatScript.isBlocking)
        {
            float blockReduction = 0.2f;

            if (EquipmentManager.instance != null && EquipmentManager.instance.isEquipped &&
                EquipmentManager.instance.currentEquippedItem != null)
            {
                blockReduction = EquipmentManager.instance.currentEquippedItem.blockReduction;
            }

            float damageToBlock = amount * blockReduction;
            finalDamage -= damageToBlock;

            combatScript.OnBlockedHit();
        }

        currentHealth -= finalDamage;

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        currentLives--;

        if (currentLives > 0)
        {
            Debug.Log($"Ноа помер. Залишилось життів: {currentLives}");
            Respawn();
        }
        else
        {
            Debug.Log("Гра закінчена — життів більше немає");
        }
    }

    private void Respawn()
    {
        currentHealth = maxHealth;

        if (respawnPoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
            if (cc != null) cc.enabled = true;

            Debug.Log("Ноа відродився на точці спавну");
        }
        else
        {
            Debug.LogError("Точка респавну не призначена в інспекторі");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth >= maxHealth && QuestManager.instance != null)
        {
            if (QuestManager.instance.IsQuestActive("quest_find_water"))
                QuestManager.instance.CompleteQuest("quest_find_water");
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => (currentHealth / maxHealth) * 100f;
}