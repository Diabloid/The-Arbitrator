using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISliderVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Смуга Fill")]
    [SerializeField] private Image _fillImage;
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 1f, 1f);
    private Color _normalColor;

    private void Awake()
    {
        if (_fillImage != null)
        {
            _normalColor = _fillImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_fillImage != null) _fillImage.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_fillImage != null) _fillImage.color = _normalColor;
    }
}