using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Компоненти")]
    public Slider slider;
    public Image fillImage;
    public Gradient gradient;

    [Header("Налаштування")]
    public float hideDelay = 10f;       // Час в секундах, через який бар буде ховатися після отримання урона

    private EntityHealth _targetHealth;
    private GameObject _canvasObj;      // Посилання на весь Канвас
    private float _lastHitTime;         // Час останнього отримання урона
    private Vector3 _originalScale;

    public void Initialize(EntityHealth target)
    {
        _targetHealth = target;

        Canvas canvas = GetComponentInParent<Canvas>();
        _canvasObj = canvas != null ? canvas.gameObject : gameObject;

        _originalScale = _canvasObj.transform.localScale;

        _targetHealth.OnHealthChanged += UpdateHealthBar;
        _targetHealth.OnDeath += OnTargetDeath;

        if (slider != null)
        {
            int max = _targetHealth.stats != null ? _targetHealth.stats.maxHealth : 100;
            slider.maxValue = max;
            slider.value = _targetHealth.currentHealth;

            if (fillImage != null && gradient != null)
                fillImage.color = gradient.Evaluate(slider.normalizedValue);
        }

        if (_canvasObj != null) _canvasObj.SetActive(false);
    }

    void OnDestroy()
    {
        if (_targetHealth != null)
        {
            _targetHealth.OnHealthChanged -= UpdateHealthBar;
            _targetHealth.OnDeath -= OnTargetDeath;
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (slider == null) return;

        slider.maxValue = max;
        slider.value = current;

        if (fillImage != null && gradient != null)
        {
            fillImage.color = gradient.Evaluate(slider.normalizedValue);
        }

        if (current >= max)
        {
            if (_canvasObj != null) _canvasObj.SetActive(false);
            return;
        }

        if (_canvasObj != null) _canvasObj.SetActive(true);
        _lastHitTime = Time.time;
    }

    private void OnTargetDeath()
    {
        if (_canvasObj != null) Destroy(_canvasObj);
    }

    void Update()
    {
        if (_canvasObj != null && _canvasObj.activeSelf)
        {
            if (Time.time > _lastHitTime + hideDelay)
            {
                _canvasObj.SetActive(false);
            }
        }
    }

    void LateUpdate()
    {
        if (_canvasObj != null && _targetHealth != null)
        {
            float direction = _targetHealth.transform.localScale.x < 0 ? -1f : 1f;

            _canvasObj.transform.localScale = new Vector3(Mathf.Abs(_originalScale.x) * direction, _originalScale.y, _originalScale.z);
        }
    }
}