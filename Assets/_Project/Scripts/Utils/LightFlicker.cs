using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [Header("Налаштування вогню")]
    [SerializeField] private float minIntensity = 0.8f; // Мінімальна яскравість
    [SerializeField] private float maxIntensity = 1.2f; // Максимальна яскравість
    [SerializeField] private float flickerSpeed = 3f;   // Швидкість горіння

    private Light2D _light;
    private float _baseRadius;
    private float _randomOffset;

    void Start()
    {
        _light = GetComponent<Light2D>();
        if (_light != null)
        {
            _baseRadius = _light.pointLightOuterRadius;
        }

        _randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (_light == null) return;

        // Генеруємо плавний шум від 0 до 1
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + _randomOffset, 0f);

        // Змінюємо яскравість
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // Змінюємо радіус (вогонь то більшає, то меншає)
        _light.pointLightOuterRadius = Mathf.Lerp(_baseRadius * 0.95f, _baseRadius * 1.05f, noise);
    }
}