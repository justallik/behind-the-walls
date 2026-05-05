using UnityEngine;
using UnityEngine.UI; // Обязательно для работы с картинками UI

/// <summary>
/// Связывает систему стамины PlayerMovement с визуальным отображением UI
/// Плавно анимирует полоску стамины
/// </summary>
public class StaminaUI : MonoBehaviour
{
    [Header("═══ ССЫЛКИ ═══")]
    public PlayerMovement playerMovement; // Ссылка на твой скрипт движения
    public Image staminaImage;            // Ссылка на картинку полоски
    
    [Header("═══ АНИМАЦИЯ ═══")]
    [SerializeField] private float fillAnimationSpeed = 5f; // Скорость плавного заполнения полоски (0-1 за N сек)

    private float targetFillAmount = 1f;
    // ⚡ ОПТИМИЗАЦИЯ: кэшируем предыдущие значения для проверки изменений
    private float lastStamina = -1f;

    void Start()
    {
        // Инициализируем цвет на натуральный (цвет из UI)
        // Больше не трогаем цвет - он не меняется
    }



    void Update()
    {
        if (playerMovement != null && staminaImage != null)
        {
            // ⚡ ОПТИМИЗАЦИЯ: обновляем только если стамина изменилась
            float currentStamina = playerMovement.GetCurrentStamina();
            if (currentStamina != lastStamina)
            {
                targetFillAmount = currentStamina / playerMovement.GetMaxStamina();
                lastStamina = currentStamina;
            }
            
            // 📊 ПЛАВНО переводим полоску в целевую позицию (вместо резкого скачка)
            staminaImage.fillAmount = Mathf.Lerp(staminaImage.fillAmount, targetFillAmount, Time.deltaTime * fillAnimationSpeed);
            
            // ✨ Цвет не меняется - натуральный UI цвет все время
        }
    }
}
