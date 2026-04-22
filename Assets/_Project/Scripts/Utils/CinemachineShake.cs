using UnityEngine;
using Unity.Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin _perlin; // Компонент шуму
    private float _shakeTimer;          // Таймер тряски
    private float _startingIntensity;   // Початкова інтенсивність тряски
    private float _shakeTimerTotal;     // Загальний час тряски

    private void Awake()
    {
        Instance = this;

        _perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (_perlin == null)
        {
            Debug.LogError("Не знайдено компонент 'CinemachineBasicMultiChannelPerlin'! Перевірьте налаштування камери.");
        }
    }

    // Метод для запуску тряски камери
    public void ShakeCamera(float intensity, float time)
    {
        if (_perlin == null)
        {
            Debug.LogWarning("Спроба тряски, але компонента Perlin немає!");
            return;
        }
        
        _perlin.AmplitudeGain = intensity;

        _startingIntensity = intensity;
        _shakeTimerTotal = time;
        _shakeTimer = time;

        Debug.Log($"Тряска камери! Сила: {intensity}, Час: {time}");
    }

    private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;

            if (_shakeTimer <= 0f)
            {
                if (_perlin != null)
                {
                    _perlin.AmplitudeGain = 0f;
                }
            }
        }
    }
}