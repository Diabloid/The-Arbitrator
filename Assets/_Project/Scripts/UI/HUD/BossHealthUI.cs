using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthUI : MonoBehaviour
{
    [Header("Налаштування Боса")]
    [SerializeField] private EntityHealth bossHealth;

    [Header("UI Елементи")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI percentText;

    [Header("Візуал та Поява")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool hideWhenDead = true;
    [SerializeField] private float fadeInDuration = 1.0f;

    private float _targetFillAmount = 1f;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        if (bossHealth == null)
        {
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null) bossHealth = bossObj.GetComponent<EntityHealth>();
        }

        if (bossHealth != null && fillImage != null)
        {
            _targetFillAmount = (float)bossHealth.currentHealth / bossHealth.stats.maxHealth;
            fillImage.fillAmount = _targetFillAmount;
        }
    }

    private void Update()
    {
        if (bossHealth == null) return;

        float maxHp = bossHealth.stats.maxHealth;
        float currentHp = bossHealth.currentHealth;

        if (maxHp > 0) _targetFillAmount = currentHp / maxHp;

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, _targetFillAmount, Time.deltaTime * smoothSpeed);
        }

        if (percentText != null)
        {
            int percent = Mathf.RoundToInt(_targetFillAmount * 100f);
            percentText.text = Mathf.Max(0, percent).ToString() + "%";
        }

        if (hideWhenDead && currentHp <= 0 && _canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha -= Time.deltaTime;
        }
    }

    // Публічний метод для появи UI
    public void ShowUI()
    {
        StartCoroutine(FadeInCoroutine());
    }

    // Публічний метод для приховування UI
    public void HideUI()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // Плавно збільшуємо прозорість від 0 до 1
            if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }
        if (_canvasGroup != null) _canvasGroup.alpha = 1f; // Гарантуємо повну видимість в кінці
    }

    private IEnumerator FadeOutRoutine()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup != null)
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                // Плавно зменшуємо прозорість від 1 до 0
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f; // Гарантуємо повну невидимість в кінці
        }

        gameObject.SetActive(false);
    }
}