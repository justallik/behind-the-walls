using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ПРОСТАЯ версия Сцена 1: Пробуждение.
/// Чёрный экран → Fade in → 5 сек ожидания → "НАЙТИ ЛАГЕРЬ" → движение включено
/// </summary>
public class WakeUpSceneController : MonoBehaviour
{
    [Header("Ссылки")]
    public GameObject player;
    public Camera playerCamera;

    [Header("UI (Canvas)")]
    public Image blurOverlay;           // Чёрная панель для затемнения
    public GameObject objectivePanel;
    public Text objectiveText;

    [Header("Параметры")]
    public float fadeInDuration = 3f;   // Длина fade in (чёрный → видно)
    public float waitDuration = 5f;     // Ждём (игрок не может двигаться)

    private PlayerMovement _playerMovement;
    private MouseMovement _mouseMovement;
    private bool _isInitialized = false;
    private bool _inputLocked = false;  // ← ФЛАГ БЛОКИРОВКИ ВВОДА

    // ← STATIC флаг для всех скриптов
    public static bool introPlaying = false;

    void Start()
    {
        if (player != null)
        {
            _playerMovement = player.GetComponent<PlayerMovement>();
            
            // Ищем MouseMovement везде - на Player, на Camera, на дочках
            _mouseMovement = player.GetComponent<MouseMovement>();
            if (_mouseMovement == null)
                _mouseMovement = player.GetComponentInChildren<MouseMovement>();
            if (_mouseMovement == null && playerCamera != null)
                _mouseMovement = playerCamera.GetComponent<MouseMovement>();
            
            Debug.Log($"✅ PlayerMovement найден: {_playerMovement != null}");
            Debug.Log($"✅ MouseMovement найден: {_mouseMovement != null}");
            if (_mouseMovement != null)
                Debug.Log($"   Находится на: {_mouseMovement.gameObject.name}");
        }

        // Начальное состояние: чёрный экран, движение выключено
        SetBlurAlpha(1f);
        HideObjective();
        DisableMovement();

        // Запускаем сцену
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // В упрощённой версии логики нет - всё в Coroutine
    }

    // ==================== ГЛАВНАЯ СЦЕНА ====================
    private IEnumerator IntroSequence()
    {
        Debug.Log("🎬 INTRO: Чёрный экран");
        yield return new WaitForSeconds(0.5f);

        // Фаза 1: Fade in (чёрный → видно) 
        Debug.Log("🎬 INTRO: Fade in...");
        yield return StartCoroutine(FadeIn());
        Debug.Log("🎬 INTRO: Fade in завершён");

        // Фаза 2: Ждём 5 секунд (игрок парализован, только смотрит)
        Debug.Log($"🎬 INTRO: Ждём 5 секунд... (inputLocked = {_inputLocked})");
        yield return new WaitForSeconds(waitDuration);
        Debug.Log("🎬 INTRO: 5 секунд истекли");

        // Фаза 3: Показываем первое задание
        Debug.Log("🎬 INTRO: Показываем НАЙТИ ЛАГЕРЬ");
        ShowObjective("НАЙТИ ЛАГЕРЬ");
        
        // ✅ Активируем первый квест
        QuestManager.instance?.ActivateQuest("quest_find_camp");

        // Фаза 4: Включаем движение
        Debug.Log("🎬 INTRO: Движение включено!");
        EnableMovement();
        _isInitialized = true;
        Debug.Log("🎬 INTRO: ЗАВЕРШЕНО");
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeInDuration;
            SetBlurAlpha(1f - t);  // От 1 (чёрно) к 0 (видно)
            yield return null;
        }
        SetBlurAlpha(0f);
    }
    private void SetBlurAlpha(float a)
    {
        if (blurOverlay != null)
        {
            Color c = blurOverlay.color;
            c.a = a;
            blurOverlay.color = c;
        }
    }

    private void ShowObjective(string text)
    {
        if (objectiveText != null) objectiveText.text = text;
        if (objectivePanel != null) objectivePanel.SetActive(true);
    }

    private void HideObjective()
    {
        if (objectivePanel != null) objectivePanel.SetActive(false);
    }

    private void DisableMovement()
    {
        Debug.Log("🔒 БЛОКИРУЕМ ВВОД (introPlaying = true)");
        introPlaying = true;
        _inputLocked = true;
        
        // На всякий случай отключаем скрипты тоже
        if (_playerMovement != null) _playerMovement.enabled = false;
        if (_mouseMovement != null) _mouseMovement.enabled = false;
    }

    private void EnableMovement()
    {
        Debug.Log("🔓 РАЗБЛОКИРУЕМ ВВОД (introPlaying = false)");
        introPlaying = false;
        _inputLocked = false;
        
        if (_playerMovement != null) _playerMovement.enabled = true;
        if (_mouseMovement != null) _mouseMovement.enabled = true;
    }
}
