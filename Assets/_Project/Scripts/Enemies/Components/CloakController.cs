using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CloakController : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Light2D _enemyLight;

    [Header("Налаштування маскування")]
    public float fadeSpeed = 8f;
    public float cloakedAlpha = 0f;

    // Внутрішні змінні
    private float _targetAlpha = 1f;
    private GameObject _dynamicHealthBar;
    private float _targetLightIntensity = 1f;
    private float _originalLightIntensity = 1f;
    private bool _isDead = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        _enemyLight = GetComponentInChildren<Light2D>();

        if (_enemyLight != null)
        {
            _originalLightIntensity = _enemyLight.intensity;
            _targetLightIntensity = _originalLightIntensity;
        }
    }

    private void Update()
    {
        if (_sr == null) return;

        if (Mathf.Abs(_sr.color.a - _targetAlpha) > 0.01f)
        {
            Color c = _sr.color;
            c.a = Mathf.Lerp(c.a, _targetAlpha, Time.deltaTime * (_isDead ? fadeSpeed * 2f : fadeSpeed));
            _sr.color = c;
        }

        if (_enemyLight != null && Mathf.Abs(_enemyLight.intensity - _targetLightIntensity) > 0.01f)
        {
            _enemyLight.intensity = Mathf.Lerp(_enemyLight.intensity, _targetLightIntensity, Time.deltaTime * (_isDead ? fadeSpeed * 2f : fadeSpeed));
        }
    }

    // Головний метод для керування маскуванням
    public void SetCloak(bool isCloaked)
    {
        if (_isDead) return;

        _targetAlpha = isCloaked ? cloakedAlpha : 1f;

        if (_enemyLight != null)
        {
            _targetLightIntensity = isCloaked ? 0f : _originalLightIntensity;
        }

        if (_dynamicHealthBar == null)
        {
            Canvas enemyUI = GetComponentInChildren<Canvas>();

            if (enemyUI != null)
            {
                _dynamicHealthBar = enemyUI.gameObject;
            }
        }

        if (_dynamicHealthBar != null)
        {
            _dynamicHealthBar.SetActive(!isCloaked);
        }
    }

    public void OnDeathReveal()
    {
        _isDead = true;
        _targetAlpha = 1f;

        if (_enemyLight != null)
        {
            _targetLightIntensity = _originalLightIntensity;
        }
    }
}