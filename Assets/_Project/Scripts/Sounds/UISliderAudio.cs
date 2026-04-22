using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class UISliderAudio : MonoBehaviour, IPointerEnterHandler, IDragHandler
{
    [Header("Звуки")]
    [SerializeField] private AudioClip _hoverSound;
    [SerializeField] private AudioClip _dragTickSound;
    [SerializeField] private float _tickStep = 0.05f;

    private Slider _slider;
    private float _lastTickValue;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && _hoverSound != null)
        {
            AudioManager.Instance.PlaySFX(_hoverSound, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Mathf.Abs(_slider.value - _lastTickValue) >= _tickStep)
        {
            _lastTickValue = _slider.value;

            if (AudioManager.Instance != null && _dragTickSound != null)
            {
                AudioManager.Instance.PlaySFXRandomPitch(_dragTickSound, 0.4f, 0.9f, 1.1f);
            }
        }
    }
}