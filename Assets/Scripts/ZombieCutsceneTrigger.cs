using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;

public class ZombieCutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene")]
    public PlayableDirector director;
    public GameObject zombieRoot; // родитель зомби

    [Header("Optional")]
    public MonoBehaviour enemyAI; // EnemyAI
    public NavMeshAgent agent;    // NavMeshAgent

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // остановить AI
        if (enemyAI != null) enemyAI.enabled = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // запуск таймлайна
        if (director != null) director.Play();
    }
}
