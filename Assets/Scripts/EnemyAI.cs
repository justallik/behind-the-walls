using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Lying, Patrol, Idle, Aggro, Chase, Attack, Die }
    public EnemyState currentState = EnemyState.Lying;
    private EnemyState previousState = EnemyState.Lying;

    [Header("AI Settings")]
    public Transform player;
    public float chaseRange = 15f;
    public float attackRange = 2f;
    public float aggroDelay = 1.0f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolStopDistance = 1f;
    [SerializeField] private float randomWaitTime = 2f;
    private float patrolWaitTimer = 0f;
    private bool patrolDestinationSet = false;
    private bool hasStartedMoving = false;
    private float lostAggroTimer = 0f;
    [SerializeField] private float lostAggroTimeout = 5f;

    [Header("Vision")]
    [SerializeField] private float visionRange = 15f;
    [SerializeField] private LayerMask visionObstacles;
    [SerializeField] private float visionAngle = 90f;
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;

    [Header("Attack")]
    public float attackDamage = 20f;
    public float attackCooldown = 3.5f;
    private float nextAttackTime = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private float attackSoundVolume = 0.7f;
    private AudioSource audioSource;

    private Animator animator;
    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private EnemyHealth myHealth;
    private StealthSystem playerStealth;
    private float aggroTimer = 0f;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError($"{gameObject.name}: NavMeshAgent не знайдено");

        myHealth = GetComponent<EnemyHealth>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("Гравця не знайдено");
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();

            playerStealth = player.GetComponent<StealthSystem>();

            if (playerHealth == null)
                Debug.LogError("PlayerHealth не знайдено на гравці");
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
            patrolPoints = new Transform[] { transform };

        lastKnownPlayerPosition = transform.position;
        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        if (player == null || agent == null) return;
        if (!agent.isOnNavMesh) return;

        if (myHealth != null && myHealth.isKnockout && currentState != EnemyState.Die)
        {
            SafeStop();
            if (animator != null) animator.SetBool("IsAggro", false);
            return;
        }

        canSeePlayer = CanSeePlayer();

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        previousState = currentState;
        switch (currentState)
        {
            case EnemyState.Lying:   UpdateLying();   break;
            case EnemyState.Patrol:  UpdatePatrol();  break;
            case EnemyState.Idle:    UpdateIdle();    break;
            case EnemyState.Aggro:   UpdateAggro();   break;
            case EnemyState.Chase:   UpdateChase();   break;
            case EnemyState.Attack:  UpdateAttack();  break;
            case EnemyState.Die:     return;
        }
    }

    private void SafeStop()
    {
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
    }

    private void SafeResume()
    {
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        bool isStealth = playerStealth != null && playerStealth.IsStealth();

        float currentVisionRange = isStealth ? visionRange * 0.5f : visionRange;
        float currentVisionAngle = isStealth ? visionAngle * 0.5f : visionAngle;

        Vector3 directionToPlayer = player.position - transform.position;
        if (directionToPlayer.sqrMagnitude > currentVisionRange * currentVisionRange) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angleToPlayer > currentVisionAngle / 2f) return false;

        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDirection = (player.position - rayOrigin).normalized;
        float rayDistance = Vector3.Distance(rayOrigin, player.position);

        if (Physics.Raycast(rayOrigin, rayDirection, rayDistance, visionObstacles)) return false;

        return true;
    }

    private void UpdateLying()
    {
        SafeStop();
        if (animator != null) animator.SetBool("IsLying", true);

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        if (canSeePlayer && sqrDist <= chaseRange * chaseRange)
            TransitionToState(EnemyState.Aggro);
    }

    private void UpdatePatrol()
    {
        float sqrDist = (player.position - transform.position).sqrMagnitude;

        if (canSeePlayer && sqrDist <= chaseRange * chaseRange)
        {
            lostAggroTimer = 0f;
            TransitionToState(EnemyState.Aggro);
            return;
        }

        if (!patrolDestinationSet)
        {
            SafeResume();
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            patrolDestinationSet = true;
            hasStartedMoving = false;
            if (animator != null) animator.SetBool("IsAggro", false);
            return;
        }

        if (agent.velocity.magnitude > 0.3f) hasStartedMoving = true;

        float sqrToPoint = (patrolPoints[currentPatrolIndex].position - transform.position).sqrMagnitude;
        bool arrived = sqrToPoint <= patrolStopDistance * patrolStopDistance;
        bool stuck = agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid;
        bool stopped = hasStartedMoving && !agent.pathPending && agent.velocity.magnitude < 0.1f;

        if (arrived || stuck || stopped)
        {
            SafeStop();
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= randomWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                patrolWaitTimer = 0f;
                patrolDestinationSet = false;
                hasStartedMoving = false;
            }
        }
    }

    private void UpdateIdle()
    {
        SafeStop();
        if (animator != null) animator.SetBool("IsAggro", false);

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        if (canSeePlayer && sqrDist <= chaseRange * chaseRange)
            TransitionToState(EnemyState.Aggro);
    }

    private void UpdateAggro()
    {
        SafeStop();
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();

        aggroTimer += Time.deltaTime;
        if (aggroTimer >= aggroDelay)
        {
            TransitionToState(EnemyState.Chase);
            return;
        }

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        float sqrExtended = (chaseRange * 1.5f) * (chaseRange * 1.5f);

        if (!canSeePlayer || sqrDist > sqrExtended)
        {
            lostAggroTimer += Time.deltaTime;
            if (lostAggroTimer >= lostAggroTimeout)
                TransitionToState(EnemyState.Patrol);
        }
        else
        {
            lostAggroTimer = 0f;
        }
    }

    private void UpdateChase()
    {
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        float sqrExtended = (chaseRange * 1.5f) * (chaseRange * 1.5f);

        if (sqrDist <= attackRange * attackRange)
        {
            if (agent != null && agent.isOnNavMesh)
                agent.stoppingDistance = attackRange * 0.8f;
            TransitionToState(EnemyState.Attack);
            return;
        }

        if (!canSeePlayer || sqrDist > sqrExtended)
        {
            SafeResume();
            agent.speed = chaseSpeed;
            agent.SetDestination(lastKnownPlayerPosition);

            if ((lastKnownPlayerPosition - transform.position).sqrMagnitude < 1.5f * 1.5f)
                TransitionToState(EnemyState.Patrol);
        }
        else
        {
            lastKnownPlayerPosition = player.position;
            SafeResume();
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.9f;
            agent.SetDestination(player.position);
            if (animator != null) animator.SetBool("IsAggro", true);
        }
    }

    private void UpdateAttack()
    {
        float sqrDist = (player.position - transform.position).sqrMagnitude;

        if (sqrDist > (attackRange * 1.5f) * (attackRange * 1.5f))
        {
            TransitionToState(EnemyState.Chase);
            return;
        }

        SafeStop();
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 360f * Time.deltaTime);
        }

        if (Time.time >= nextAttackTime && !isAttacking)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        PlaySoundEffect(GetRandomAttackSound());

        yield return new WaitForSeconds(0.9f);

        if (currentState == EnemyState.Attack && playerHealth != null)
        {
            float distSqr = (player.position - transform.position).sqrMagnitude;
            if (distSqr <= (attackRange * 2f) * (attackRange * 2f))
                playerHealth.TakeDamage(attackDamage);
        }

        isAttacking = false;
        attackCoroutine = null;
    }

    private void TransitionToState(EnemyState newState)
    {
        if (currentState == newState) return;

        isAttacking = false;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        EnemyState oldState = currentState;
        previousState = currentState;
        currentState = newState;
        aggroTimer = 0f;
        lostAggroTimer = 0f;
        patrolDestinationSet = false;

        if (animator == null) return;

        animator.SetBool("IsLying", newState == EnemyState.Lying);
        animator.SetBool("IsAggro", newState == EnemyState.Chase || newState == EnemyState.Attack);

        if (oldState == EnemyState.Attack && newState != EnemyState.Attack)
            animator.ResetTrigger("Attack");

        if (newState == EnemyState.Die)
        {
            animator.SetTrigger("Die");
            Debug.Log($"{gameObject.name}: перехід у стан Die");
        }
    }

    public void DealDamage()
    {
        if (currentState != EnemyState.Attack) return;
        if (playerHealth == null) return;

        float distSqr = (player.position - transform.position).sqrMagnitude;
        if (distSqr > (attackRange * 2f) * (attackRange * 2f)) return;

        playerHealth.TakeDamage(attackDamage);
    }

    public void SetDead()
    {
        TransitionToState(EnemyState.Die);
        SafeStop();
    }

    private AudioClip GetRandomAttackSound()
    {
        if (attackSounds == null || attackSounds.Length == 0) return null;
        return attackSounds[Random.Range(0, attackSounds.Length)];
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, attackSoundVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{gameObject.name}\n{currentState}");
#endif
    }
}