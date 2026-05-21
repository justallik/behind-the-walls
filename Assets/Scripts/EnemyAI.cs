using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// ИИ враг с системой состояния (State Machine):
/// Lying → Patrol → Idle → Aggro → Chase → Attack
/// Включает стелс систему, разумную навигацию, интеграцию с боевом
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // ==================== STATE MACHINE ====================
    public enum EnemyState { Lying, Patrol, Idle, Aggro, Chase, Attack, Die }
    private EnemyState currentState = EnemyState.Lying;
    private EnemyState previousState = EnemyState.Lying;

    [Header("Настройки ИИ")]
    public Transform player;              
    public float chaseRange = 15f;        
    public float attackRange = 2f;        
    public float aggroDelay = 1.0f;

    [Header("Патруль")]
    [SerializeField] private Transform[] patrolPoints;    // Точки патруля
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5.5f;     // Максимальная скорость бега
    [SerializeField] private float patrolStopDistance = 1f;
    [SerializeField] private float randomWaitTime = 2f;
    private float patrolWaitTimer = 0f;
    private bool patrolDestinationSet = false;
    private bool hasStartedMoving = false;
    private float lostAggaroTimer = 0f;
    [SerializeField] private float lostAggroTimeout = 5f; // Время забывания игрока

    [Header("Стелс система")]
    [SerializeField] private float visionRange = 15f;     // Дальность видимости
    [SerializeField] private LayerMask visionObstacles;   // Слои что блокируют видимость (стены)
    [SerializeField] private float visionAngle = 90f;     // Угол видимости (90° = полусфера)
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;

    [Header("Настройки Атаки")]
    [SerializeField] private ItemData weaponData;         // Оружие врага (для характеристик)
    public float attackDamage = 20f;      
    public float attackCooldown = 1.5f;   
    private float nextAttackTime = 0f;

    [Header("Звуки")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private float attackSoundVolume = 0.7f;
    private AudioSource audioSource;

    [Header("Анимации")]
    private Animator animator;
    private bool hasPlayedRoar = false;

    // ==================== КОМПОНЕНТЫ ====================
    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private EnemyHealth myHealth;
    private StealthSystem playerStealth;
    private float aggroTimer = 0f;

    private void Start()
    {
        // === ИНИЦИАЛИЗАЦИЯ КОМПОНЕНТОВ ===
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError($"❌ {gameObject.name}: NavMeshAgent не найден!");

        myHealth = GetComponent<EnemyHealth>();
        if (myHealth == null)
            Debug.LogError($"❌ {gameObject.name}: EnemyHealth не найден!");

        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // === ПОИСК ИГРОКА ===
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = player.GetComponent<PlayerHealth>();
                playerStealth = player.GetComponent<StealthSystem>();
            }
            else
                Debug.LogError("❌ Игрок не найден (нет тега 'Player')!");
        }
        // Проверка наличия PlayerHealth
        if (playerHealth == null)
            Debug.LogError("❌ PlayerHealth НЕ НАЙДЕН на игроке!");
        else
            Debug.Log("✅ PlayerHealth найден!");

        // === ПАТРОЛЬНЫЕ ТОЧКИ ===
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Патрольные точки не назначены! Враг будет стоять на месте.");
            patrolPoints = new Transform[] { transform };
        }

        // === ОРУЖИЕ ВРАГА ===
        if (weaponData != null)
        {
            attackDamage = weaponData.weaponDamage;
            attackCooldown = weaponData.attackStaminaCost > 0 ? weaponData.attackStaminaCost / 10f : 1.5f;
            Debug.Log($"🔫 {gameObject.name} вооружён: {weaponData.itemName} (урон: {attackDamage})");
        }

        lastKnownPlayerPosition = transform.position;
        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        // === ПРОВЕРКА ПРЕДУСЛОВИЙ ===
        if (player == null || agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Враг не на NavMesh!");
            return;
        }

        // === ПРОВЕРКА ЗДОРОВЬЯ (НОКАУТ / СМЕРТЬ) ===
        if (myHealth != null)
        {
            if (myHealth.isKnockout && currentState != EnemyState.Die)
            {
                // В нокауте враг ничего не делает
                agent.isStopped = true;
                if (animator != null)
                    animator.SetBool("IsAggro", false);
                return;
            }
        }

        // === ПРОВЕРКА ВИДИМОСТИ (СТЕЛС СИСТЕМА) ===
        canSeePlayer = CanSeePlayer();

        // === ОБНОВЛЕНИЕ АНИМАЦИИ (СКОРОСТЬ) ===
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        // === STATE MACHINE ===
        previousState = currentState;
        switch (currentState)
        {
            case EnemyState.Lying:      UpdateLying();   break;
            case EnemyState.Patrol:     UpdatePatrol();  break;
            case EnemyState.Idle:       UpdateIdle();    break;
            case EnemyState.Aggro:      UpdateAggro();   break;
            case EnemyState.Chase:      UpdateChase();   break;
            case EnemyState.Attack:     UpdateAttack();  break;
            case EnemyState.Die:        return;
        }
    }

    // ==================== СТЕЛС: ВИДИМОСТЬ ИГРОКА ====================
    /// <summary>
    /// Проверяет видит ли враг игрока:
    /// - Расстояние в пределах visionRange
    /// - Угол видимости (если игрок в стелсе)
    /// - Нет препятствий (стены/объекты) между врагом и игроком
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        // === ПРОВЕРКА РАССТОЯНИЯ ===
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrVisionRange = visionRange * visionRange;
        
        if (sqrDistanceToPlayer > sqrVisionRange)
            return false;

        // === ПРОВЕРКА СТЕЛСА ИГРОКА ===
        if (playerStealth != null && playerStealth.IsStealth())
        {
            // В стелсе игрок должен быть в конусе видимости врага
            Vector3 normalizedDirection = directionToPlayer.normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, normalizedDirection);
            if (angleToPlayer > visionAngle / 2f)
                return false; // Игрок вне конуса видимости
        }

        // === ПРОВЕРКА ПРЕПЯТСТВИЙ (RAYCAST) ===
        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDirection = (player.position - rayOrigin).normalized;
        float rayDistance = Vector3.Distance(rayOrigin, player.position);

        if (Physics.Raycast(rayOrigin, rayDirection, rayDistance, visionObstacles))
            return false; // Что-то блокирует видимость

        return true;
    }

    // ==================== STATE: LYING (ЛЕЖИТ) ====================
    private void UpdateLying()
    {
        agent.isStopped = true;
        if (animator != null)
            animator.SetBool("IsLying", true);

        // Враг лежит и слушает - если видит игрока в диапазоне чейса, встаёт
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrChaseRange = chaseRange * chaseRange;

        if (canSeePlayer && sqrDistanceToPlayer <= sqrChaseRange)
        {
            TransitionToState(EnemyState.Aggro);
        }
    }

    // ==================== STATE: PATROL (ПАТРУЛЬ) ====================
    private void UpdatePatrol()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrChaseRange = chaseRange * chaseRange;

        if (canSeePlayer && sqrDistanceToPlayer <= sqrChaseRange)
        {
            lostAggaroTimer = 0f;
            TransitionToState(EnemyState.Aggro);
            return;
        }

        // === НАВИГАЦИЯ ===
        if (!patrolDestinationSet)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            patrolDestinationSet = true;
            hasStartedMoving = false;
            if (animator != null)
                animator.SetBool("IsAggro", false);
            return;
        }

        // Фиксируем что агент реально пошёл (для корректной остановки)
        if (agent.velocity.magnitude > 0.3f)
            hasStartedMoving = true;

        // === ПРОВЕРКА ПРИБЫТИЯ / ЗАСТРЕВАНИЯ ===
        Vector3 toPatrolPoint = patrolPoints[currentPatrolIndex].position - transform.position;
        float sqrDistToPoint = toPatrolPoint.sqrMagnitude;
        float sqrStopDistance = patrolStopDistance * patrolStopDistance;
        
        bool arrived = sqrDistToPoint <= sqrStopDistance;
        bool stuck = agent.pathStatus == NavMeshPathStatus.PathPartial
                  || agent.pathStatus == NavMeshPathStatus.PathInvalid;
        bool agentStopped = hasStartedMoving
                         && !agent.pathPending
                         && agent.velocity.magnitude < 0.1f;

        if (arrived || stuck || agentStopped)
        {
            agent.isStopped = true;

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

    // ==================== STATE: IDLE (ЖДЕТ) ====================
    private void UpdateIdle()
    {
        agent.isStopped = true;
        if (animator != null)
            animator.SetBool("IsAggro", false);

        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrChaseRange = chaseRange * chaseRange;

        if (canSeePlayer && sqrDistanceToPlayer <= sqrChaseRange)
            TransitionToState(EnemyState.Aggro);
    }

    // ==================== STATE: AGGRO (АГРО - РЫЧАНИЕ) ====================
    private void UpdateAggro()
    {
        agent.isStopped = true;
        agent.ResetPath(); // ВАЖНО: сбрасывает текущее движение

        // Играем Roar один раз при входе в Aggro
        if (!hasPlayedRoar && animator != null)
        {
            animator.SetTrigger("Roar");
            PlaySoundEffect(roarSound);
            hasPlayedRoar = true;
            Debug.Log($"🗣️ {gameObject.name} издаёт рык!");
        }

        aggroTimer += Time.deltaTime;
        if (aggroTimer >= aggroDelay)
        {
            TransitionToState(EnemyState.Chase);
            return;
        }

        // === ПРОВЕРКА ПОТЕРИ ЦЕЛИ ===
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrChaseRangeExtended = (chaseRange * 1.5f) * (chaseRange * 1.5f);

        if (!canSeePlayer || sqrDistanceToPlayer > sqrChaseRangeExtended)
        {
            lostAggaroTimer += Time.deltaTime;
            if (lostAggaroTimer >= lostAggroTimeout)
            {
                TransitionToState(EnemyState.Patrol);
                Debug.Log($"👁️ {gameObject.name} потерял цель и вернулся в патруль");
            }
        }
        else
        {
            lostAggaroTimer = 0f;
        }
    }

    // ==================== STATE: CHASE (ПОГОНЯ) ====================
    private void UpdateChase()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrAttackRange = attackRange * attackRange;
        float sqrChaseRangeExtended = (chaseRange * 1.5f) * (chaseRange * 1.5f);

        // === ПЕРЕХОД В АТАКУ ===
        if (sqrDistanceToPlayer <= sqrAttackRange)
        {
            agent.stoppingDistance = attackRange * 0.8f;
            TransitionToState(EnemyState.Attack);
            return;
        }

        // === ПОТЕРЯ ЦЕЛИ ===
        if (!canSeePlayer || sqrDistanceToPlayer > sqrChaseRangeExtended)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(lastKnownPlayerPosition);

            Vector3 toLastKnown = lastKnownPlayerPosition - transform.position;
            float sqrDistToLastKnown = toLastKnown.sqrMagnitude;
            if (sqrDistToLastKnown < 1.5f * 1.5f) // дошли до последней позиции
            {
                TransitionToState(EnemyState.Patrol);
                Debug.Log($"🚶 {gameObject.name} потерял игрока и возвращается в патруль");
            }
        }
        else
        {
            // === АКТИВНАЯ ПОГОНЯ ===
            lastKnownPlayerPosition = player.position;
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.9f; // Правильная дистанция остановки
            agent.SetDestination(player.position);
            
            if (animator != null)
                animator.SetBool("IsAggro", true);
        }
    }

    // ==================== STATE: ATTACK ====================
    private bool isAttacking = false;

    private void UpdateAttack()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float sqrDistanceToPlayer = directionToPlayer.sqrMagnitude;
        float sqrAttackRangeExtended = (attackRange * 1.5f) * (attackRange * 1.5f);

        if (sqrDistanceToPlayer > sqrAttackRangeExtended)
        {
            TransitionToState(EnemyState.Chase);
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();

        // Поворот к игроку
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 360f * Time.deltaTime);
        }

        // Атака по кулдауну
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Запускаем анимацию
        if (animator != null)
            animator.SetTrigger("Attack");

        PlaySoundEffect(GetRandomAttackSound());

        // Ждём момент удара (примерно половина анимации)
        // zombieatak1 длится 3.333 сек → удар примерно на 0.8-1.0 сек
        yield return new WaitForSeconds(0.9f);

        // Наносим урон только если всё ещё в состоянии атаки и игрок рядом
        if (currentState == EnemyState.Attack && playerHealth != null)
        {
            float distSqr = (player.position - transform.position).sqrMagnitude;
            if (distSqr <= (attackRange * 2f) * (attackRange * 2f))
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"⚔️ {gameObject.name} наносит урон: {attackDamage}");
            }
        }

        isAttacking = false;
    }

    // ==================== ПЕРЕХОДЫ СОСТОЯНИЙ ====================
    private void TransitionToState(EnemyState newState)
    {
        if (currentState == newState) return;

        // Сбрасываем флаг атаки при любом переходе
        isAttacking = false;
        StopCoroutine(nameof(AttackRoutine)); // останавливаем корутину если она идёт

        EnemyState oldState = currentState; // сохраняем ДО изменения
        previousState = currentState;
        currentState = newState;
        aggroTimer = 0f;
        lostAggaroTimer = 0f;
        patrolDestinationSet = false;

        if (animator == null) return;

        animator.SetBool("IsLying", newState == EnemyState.Lying);
        animator.SetBool("IsAggro", newState == EnemyState.Chase || newState == EnemyState.Attack);

        // Сброс триггеров при выходе из атаки
        if (oldState == EnemyState.Attack && newState != EnemyState.Attack)
        {
            animator.ResetTrigger("Attack");
        }

        if (newState == EnemyState.Aggro)
        {
            hasPlayedRoar = false;
            Debug.Log($"😤 {gameObject.name}: Переход в AGGRO");
        }

        if (newState == EnemyState.Die)
        {
            animator.SetTrigger("Die");
            Debug.Log($"💀 {gameObject.name}: Переход в DIE");
        }

        Debug.Log($"🔄 {gameObject.name}: {previousState} → {newState}");
    }

    // ==================== УРОН (ВЫЗЫВАЕТСЯ ИЗ АНИМАЦИИ) ====================
    /// <summary>
    /// Метод вызывается из события анимации удара (Attack Animation Event)
    /// чтобы урон наносился строго в момент контакта, а не постоянно
    /// </summary>
    public void DealDamage()
    {
        if (currentState != EnemyState.Attack) return; // не в состоянии атаки — игнорируем
        if (playerHealth == null) return;

        float distSqr = (player.position - transform.position).sqrMagnitude;
        if (distSqr > (attackRange * 2f) * (attackRange * 2f)) return; // игрок уже убежал

        playerHealth.TakeDamage(attackDamage);
        Debug.Log($"⚔️ {gameObject.name} наносит урон: {attackDamage}");
    }

    // ==================== СМЕРТЬ ====================
    public void SetDead()
    {
        TransitionToState(EnemyState.Die);
        if (agent != null)
            agent.isStopped = true;
    }

    // ==================== СИСТЕМА ЗВУКОВ ====================
    private AudioClip GetRandomAttackSound()
    {
        if (attackSounds == null || attackSounds.Length == 0)
            return null;

        return attackSounds[Random.Range(0, attackSounds.Length)];
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.PlayOneShot(clip, attackSoundVolume);
    }

    private void OnDrawGizmosSelected()
    {
        // === ВИДИМОСТЬ (голубая сфера) ===
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // === ДАЛЬНОСТЬ АТАКИ (красная сфера) ===
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // === ДАЛЬНОСТЬ ПОГОНИ (жёлтая сфера) ===
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // === КОНУС ВИДИМОСТИ (если есть стелс) ===
        if (visionAngle > 0)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Vector3 forward = transform.forward;
            Vector3 left = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward * visionRange;
            Vector3 right = Quaternion.Euler(0, visionAngle / 2f, 0) * forward * visionRange;
            
            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + right);
        }

        // === ПАТРОЛЬНЫЕ ТОЧКИ (зелёные) ===
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    
                    // Линия между точками патруля
                    Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];
                    if (nextPoint != null)
                        Gizmos.DrawLine(patrolPoints[i].position, nextPoint.position);
                    
                    // Линия от врага к первой точке
                    if (i == 0)
                        Gizmos.DrawLine(transform.position, patrolPoints[i].position);
                }
            }
        }

        // === ПОСЛЕДНЯЯ ИЗВЕСТНАЯ ПОЗИЦИЯ (синяя) ===
        if (currentState == EnemyState.Chase || currentState == EnemyState.Patrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.25f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }

        // === ТЕКУЩЕЕ СОСТОЯНИЕ ===
        string stateText = $"{gameObject.name}\n{currentState}";
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, stateText);
        #endif
    }
}