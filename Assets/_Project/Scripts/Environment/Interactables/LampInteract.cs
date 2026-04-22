using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LampInteract : MonoBehaviour, IInteractable
{
    [Header("UI Підказка")]
    [SerializeField] private float promptHeight = 0.5f;

    [Header("Світло та Візуал")]
    [SerializeField] private Light2D lampLight;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;

    [Header("Звуки")]
    [SerializeField] private AudioClip _soundTurnOn;
    [SerializeField] private AudioClip _soundTurnOff;

    private bool _isOn = true;

    // Контракти інтерфейсу
    public string InteractionPrompt => _isOn ? "Вимкнути лампу" : "Увімкнути лампу";
    public float PromptHeight => promptHeight;

    private void Start()
    {
        if (lampLight == null) lampLight = GetComponent<Light2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisuals();
    }

    public void Interact()
    {
        _isOn = !_isOn;
        UpdateVisuals();

        if (AudioManager.Instance != null)
        {
            if (_isOn && _soundTurnOn != null)
            {
                // Звук увімкнення
                AudioManager.Instance.PlaySFXRandomPitch(_soundTurnOn, 0.7f, 0.9f, 1.1f);
            }
            else if (!_isOn && _soundTurnOff != null)
            {
                // Звук вимкнення
                AudioManager.Instance.PlaySFXRandomPitch(_soundTurnOff, 1.2f, 0.9f, 1.1f);
            }
        }
    }

    // Інкапсулюємо логіку візуалу
    private void UpdateVisuals()
    {
        if (lampLight != null) lampLight.enabled = _isOn;

        if (spriteRenderer != null && spriteOn != null && spriteOff != null)
        {
            spriteRenderer.sprite = _isOn ? spriteOn : spriteOff;
        }
    }
}