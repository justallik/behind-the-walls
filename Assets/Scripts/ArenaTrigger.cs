using UnityEngine;

public class ArenaTrigger : MonoBehaviour
{
    [Header("Zombie")]
    [SerializeField] private GameObject zombieObject;

    [Header("Return Point")]
    [SerializeField] private Transform returnPoint;

    private EnemyAI enemyAI;
    private bool encounterActive = false;

    private void Start()
    {
        if (zombieObject != null)
            enemyAI = zombieObject.GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if (zombieObject != null && zombieObject.activeSelf && !encounterActive)
            encounterActive = true;

        if (encounterActive && enemyAI != null && enemyAI.currentState == EnemyAI.EnemyState.Die)
            encounterActive = false;
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