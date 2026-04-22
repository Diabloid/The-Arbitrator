using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Звуки")]
    [SerializeField] private AudioClip _hoverSound;
    [SerializeField] private AudioClip _clickSound;

    // Спрацьовує, коли мишка наводиться на кнопку
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && _hoverSound != null)
        {
            AudioManager.Instance.PlaySFX(_hoverSound, 0.5f);
        }
    }

    // Спрацьовує, коли мишка клікає по кнопці
    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && _clickSound != null)
        {
            AudioManager.Instance.PlaySFX(_clickSound, 1f);
        }
    }
}