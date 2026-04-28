using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Компоненты")]
    public PlayerInputHandler inputHandler;
    public Camera playerCamera;
    public PlayerMovement playerMovement;

    [Header("Настройки боевки")]
    public float attackRange = 2.5f;
    public float attackCooldown = 0.6f;
    public float superAttackCooldown = 3.0f; 
    public float dodgeStaminaCost = 25f;

    [Header("Настройки блока")]
    [SerializeField] private int maxBlockHits = 2; // Блок ломается после N ударов врага
    private int blockHitsAbsorbed = 0; // Счётчик заблокированных ударов
    private bool canBlockAgain = true; // Чтобы нельзя было спамить блок, не отпуская кнопку

    private float lastAttackTime = 0f;
    private float lastSuperAttackTime = 0f;
    
    [HideInInspector] public bool isBlocking = false;

    void Start()
    {
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (inputHandler == null || playerMovement == null) return;

        // 1. УКЛОНЕНИЕ (Всегда работает)
        CheckCrouchingDodgeInput();

        // 2. ОБНОВЛЕННАЯ ЛОГИКА БЛОКА
        HandleBlockLogic();

        // 3. ДОБИВАЮЩИЙ УДАР (F)
        CheckExecuteInput();

        // --- ПРОВЕРКА ОРУЖИЯ ДЛЯ АТАК ---
        bool hasWeapon = EquipmentManager.instance != null && 
                         EquipmentManager.instance.isEquipped && 
                         EquipmentManager.instance.currentEquippedItem != null;

        if (!hasWeapon) return; 

        // 4. ОБЫЧНЫЙ УДАР (ЛКМ)
        if (inputHandler.AttackInput && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }

        // 5. СУПЕР УДАР (Клавиша V)
        if (Keyboard.current.vKey.wasPressedThisFrame && Time.time >= lastSuperAttackTime + superAttackCooldown)
        {
            PerformSuperAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        ItemData weaponData = EquipmentManager.instance.currentEquippedItem;

        Debug.Log("🗡 Обычный взмах ножом");
        PlayWeaponAnimation("Attack");
        
        // ЛОМАЕМ СТЕЛС при атаке
        StealthSystem stealth = GetComponent<StealthSystem>();
        if (stealth != null) stealth.BreakStealth();
        
        if (weaponData != null) CheckHit(weaponData.weaponDamage, false); 
    }

    private void CheckCrouchingDodgeInput()
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            Vector3 dodgeDir = Vector3.zero;

            if (Keyboard.current.aKey.wasPressedThisFrame) dodgeDir = -playerMovement.transform.right;
            else if (Keyboard.current.dKey.wasPressedThisFrame) dodgeDir = playerMovement.transform.right;
            else if (Keyboard.current.sKey.wasPressedThisFrame) dodgeDir = -playerMovement.transform.forward; // 🔄 НАЗАД

            if (dodgeDir != Vector3.zero)
            {
                if (playerMovement.HasEnoughStamina(dodgeStaminaCost))
                {
                    playerMovement.UseStamina(dodgeStaminaCost);
                    playerMovement.PerformCrouchingDodge(dodgeDir);
                    Debug.Log("💨 Уклонение!");
                    PlayWeaponAnimation("Dodge");
                    
                    // ✨ АКТИВИРУЕМ I-ФРЕЙМЫ
                    PlayerHealth playerHealth = GetComponent<PlayerHealth>();
                    if (playerHealth != null) playerHealth.StartIFrames();
                }
                else playerMovement.TriggerExhaustion();
            }
        }
    }

    private void HandleBlockLogic()
    {
        // Если игрок отпустил кнопку блока — разрешаем блокировать снова
        if (!inputHandler.BlockInput)
        {
            canBlockAgain = true;
            if (isBlocking) StopBlock();
            return;
        }

        // Если кнопка зажата, стамины хватает и блок еще не сломан
        if (inputHandler.BlockInput && canBlockAgain && playerMovement.GetCurrentStamina() > 0 && blockHitsAbsorbed < maxBlockHits)
        {
            if (!isBlocking) 
            {
                StartBlock();
            }

            // Тратим стамину на блокировку
            float staminaCost = 15f; 
            if (EquipmentManager.instance.isEquipped && EquipmentManager.instance.currentEquippedItem != null)
            {
                staminaCost = EquipmentManager.instance.currentEquippedItem.blockStaminaCost;
            }
            playerMovement.UseStamina(staminaCost * Time.deltaTime);
        }
        else if (isBlocking)
        {
            StopBlock();
        }
    }

    private void StartBlock()
    {
        isBlocking = true;
        blockHitsAbsorbed = 0; // Сброс счётчика ударов при новом блоке
        Debug.Log("🛡 Начали блок");
    }

    private void StopBlock()
    {
        if (isBlocking)
        {
            isBlocking = false;
            blockHitsAbsorbed = 0;
            Debug.Log("🛡 Сняли блок");
        }
    }

    // ==================== ВЫЗЫВАЕТСЯ ИЗ PlayerHealth КОГДА ВРАГ ПОПАДАЕТ В БЛОК ====================
    public void OnBlockedHit()
    {
        if (!isBlocking) return;

        blockHitsAbsorbed++;
        Debug.Log($"💢 Блокирован удар: {blockHitsAbsorbed}/{maxBlockHits}");

        if (blockHitsAbsorbed >= maxBlockHits)
        {
            Debug.Log("💔 Блок сломан! Враг пробил защиту!");
            StopBlock();
            canBlockAgain = false; // Нужно отпустить и снова нажать RMB
        }
    }

    // ==================== ДОБИВАЮЩИЙ УДАР (F) ====================
    private void CheckExecuteInput()
    {
        if (!Keyboard.current.fKey.wasPressedThisFrame) return;

        // Ищем врага в радиусе 2.5м
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        
        foreach (Collider collider in hits)
        {
            EnemyHealth enemy = collider.GetComponentInParent<EnemyHealth>();
            if (enemy == null) continue;

            // Проверяем если враг ослаблен (<30% HP)
            float healthPercent = (enemy.GetCurrentHealth() / enemy.maxHealth) * 100f;
            
            if (healthPercent < 30f)
            {
                Debug.Log($"⚡ ВЫПОЛНЕНИЕ! Враг повержен! HP: {healthPercent:F1}%");
                PlayWeaponAnimation("Execute");
                enemy.TakeDamage(enemy.maxHealth); // Мгновенная смерть
                return; // Выполняем только на первого врага в радиусе
            }
        }

        Debug.Log("❌ Враг слишком здоров для добивающего удара!");
    }

    private void PerformSuperAttack()
    {
        float superCost = 40f; 
        if (playerMovement.HasEnoughStamina(superCost))
        {
            playerMovement.UseStamina(superCost);
            lastSuperAttackTime = Time.time;

            Debug.Log("💥 СУПЕР-УДАР: Нож + Кулак (V)!");
            PlayWeaponAnimation("SuperAttack");
            
            // ЛОМАЕМ СТЕЛС при супер-ударе
            StealthSystem stealth = GetComponent<StealthSystem>();
            if (stealth != null) stealth.BreakStealth();
            
            ItemData weaponData = EquipmentManager.instance.currentEquippedItem;
            if (weaponData != null) CheckHit(weaponData.weaponDamage * 2f, true);
        }
        else playerMovement.TriggerExhaustion();
    }

    private void CheckHit(float damage, bool isKnockout)
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (isKnockout) enemy.ApplyKnockout(3.0f);
                Debug.Log($"🎯 Попали по врагу! Нанесен урон: {damage}");
            }
        }
    }

    private void PlayWeaponAnimation(string triggerName)
    {
        GameObject activeWeapon = EquipmentManager.instance.GetActiveWeaponObject();
        if (activeWeapon != null)
        {
            Animator anim = activeWeapon.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger(triggerName);
        }
    }
}
