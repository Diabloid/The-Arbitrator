using UnityEngine;
using TMPro;
using System.Collections;

public class CurrencyUI : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private PlayerStats stats;         // Посилання на дані
    [SerializeField] private TextMeshProUGUI coinText;  // Посилання на текст

    [Header("Анімація Лічильника")]
    [SerializeField] private float popScale = 1.3f;             // Наскільки сильно збільшується текст при підборі (1.0 - стандарт)
    [SerializeField] private float maxAnimationDuration = 0.5f; // Максимальний час накручування цифр

    private float _displayedValue; // Поточне відображуване значення
    private Coroutine _countingCoroutine;
    private Vector3 _originalScale;

    private void Awake()
    {
        if (coinText != null)
        {
            _originalScale = coinText.transform.localScale;
        }
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnCurrencyChanged += UpdateUI;

            // На старті показуємо одразу фінальне число
            _displayedValue = stats.currentCurrency;
            coinText.text = stats.currentCurrency.ToString();
        }
    }

    private void OnDisable()
    {
        if (stats != null) stats.OnCurrencyChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (_countingCoroutine != null) StopCoroutine(_countingCoroutine);

        _countingCoroutine = StartCoroutine(CountAndPop(stats.currentCurrency));
    }

    private IEnumerator CountAndPop(int targetValue)
    {
        float startValue = _displayedValue;
        float difference = Mathf.Abs(targetValue - startValue);

        float actualDuration = Mathf.Clamp(difference * 0.05f, 0.1f, maxAnimationDuration);

        float timer = 0f;

        // Робимо різкий "Пуф" (збільшуємо текст)
        coinText.transform.localScale = _originalScale * popScale;

        while (timer < actualDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / actualDuration;

            _displayedValue = Mathf.Lerp(startValue, targetValue, progress);
            coinText.text = Mathf.RoundToInt(_displayedValue).ToString();

            // Плавно здуваємо текст назад до оригінального розміру
            coinText.transform.localScale = Vector3.Lerp(_originalScale * popScale, _originalScale, progress);

            yield return null;
        }

        _displayedValue = targetValue;
        coinText.text = targetValue.ToString();
        coinText.transform.localScale = _originalScale;
    }
}