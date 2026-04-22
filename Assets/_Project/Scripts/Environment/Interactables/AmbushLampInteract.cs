using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class AmbushLampInteract : MonoBehaviour, IInteractable
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

    [Header("Секретна Засідка (Миші)")]
    [SerializeField] private GameObject[] batsToWake;
    private bool _ambushTriggered = false;
    private bool _isOn = false;

    [Header("Події")]
    public UnityEvent onLampTurnedOn;
    public UnityEvent onLampTurnedOff;

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

        // Засідка спрацьовує лише один раз при першому увімкненні
        if (_isOn && !_ambushTriggered && batsToWake.Length > 0)
        {
            TriggerBatAmbush();
            onLampTurnedOn?.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        if (lampLight != null) lampLight.enabled = _isOn;

        if (spriteRenderer != null && spriteOn != null && spriteOff != null)
        {
            spriteRenderer.sprite = _isOn ? spriteOn : spriteOff;
        }
    }

    private void TriggerBatAmbush()
    {
        _ambushTriggered = true;

        foreach (GameObject bat in batsToWake)
        {
            if (bat != null)
            {
                SpriteRenderer sr = bat.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;

                Animator anim = bat.GetComponentInChildren<Animator>();
                if (anim != null) anim.SetTrigger("WakeUp");
            }
        }
        Debug.Log("Світло увімкнено. Летючі миші прокинулись!");
    }
}