using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextColor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("Кольори")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = Color.gray;

    private Button _btn;

    private void Start()
    {
        _btn = GetComponent<Button>();
    }

    private void Update()
    {
        if (_btn != null && !_btn.interactable)
        {
            if (_buttonText != null) _buttonText.color = _pressedColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_btn != null && _btn.interactable && _buttonText != null)
            _buttonText.color = _pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_btn != null && _btn.interactable && _buttonText != null)
            _buttonText.color = _normalColor;
    }
}