using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isKnockout = false;

    [Header("Death")]
    public int diaryEntryOnDeath = 11;

    [Header("Quests")]
    [SerializeField] private string questToCompleteOnDeath = "quest_survive";
    [SerializeField] private string questToActivateOnDeath = "quest_find_water";

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    public void ApplyKnockout(float duration)
    {
        if (isKnockout) return;
        StartCoroutine(KnockoutRoutine(duration));
    }

    private IEnumerator KnockoutRoutine(float duration)
    {
        isKnockout = true;
        Debug.Log($"{gameObject.name} в нокауті");
        yield return new WaitForSeconds(duration);
        isKnockout = false;
        Debug.Log($"{gameObject.name} прийшов до тями");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} переможено");

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.SetDead();

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        if (DiaryManager.instance != null && diaryEntryOnDeath > 0)
            DiaryManager.instance.AddEntryByID(diaryEntryOnDeath);

        if (QuestManager.instance != null)
        {
            if (!string.IsNullOrEmpty(questToCompleteOnDeath))
                QuestManager.instance.CompleteQuest(questToCompleteOnDeath);

            if (!string.IsNullOrEmpty(questToActivateOnDeath))
                QuestManager.instance.ActivateQuest(questToActivateOnDeath);
        }
    }

    public float GetCurrentHealth() => currentHealth;
}