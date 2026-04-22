using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ChoiceHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Налаштування кольору")]
    [SerializeField] private Color hoverColor = Color.red;

    private TMP_Text _buttonText;
    private Color _originalColor;

    private void Awake()
    {
        _buttonText = GetComponentInChildren<TMP_Text>();

        if (_buttonText != null)
        {
            _originalColor = _buttonText.color;
        }
    }

    // Спрацьовує в той кадр, коли курсор торкається колайдера кнопки
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonText != null)
        {
            _buttonText.color = hoverColor;
        }
    }

    // Спрацьовує, коли курсор залишає кнопку
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonText != null)
        {
            _buttonText.color = _originalColor;
        }
    }
}