using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SaveUIBinder : MonoBehaviour
{
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.saveNotification = GetComponent<CanvasGroup>();
            Debug.Log("UI Збереження успішно прив'язано до SaveManager.");
        }
        else
        {
            Debug.LogWarning("SaveManager не знайдено.");
        }
    }
}