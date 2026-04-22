using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private float promptHeight = 0.5f;
    public float PromptHeight => promptHeight;

    [Header("Звуки")]
    [SerializeField] private AudioClip _lockedSound;
    [SerializeField] private AudioClip _openSound;

    // Динамічна підказка
    public string InteractionPrompt
    {
        get
        {
            // Якщо є взята місія - показуємо куди йдемо
            if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
            {
                return $"Відправитись в {MissionManager.Instance.currentMission.locationName}";
            }
            // Якщо місії немає - двері зачинені
            return "Двері зачинені (Потрібна Справа)";
        }
    }

    public void Interact()
    {
        if (MissionManager.Instance == null || MissionManager.Instance.currentMission == null)
        {
            Debug.Log("Вам потрібен офіційний Мандат від Імперії.");

            if (AudioManager.Instance != null && _lockedSound != null)
            {
                AudioManager.Instance.PlaySFXRandomPitch(_lockedSound, 0.8f, 0.95f, 1.05f);
            }

            return;
        }
        if (AudioManager.Instance != null && _openSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_openSound, 1.0f, 0.9f, 1.1f);
        }

        string targetScene = MissionManager.Instance.currentMission.sceneToLoad;
        Debug.Log($"Відправляємось на завдання... Завантажуємо сцену: {targetScene}");

        LoadingScreenManager.Instance.LoadScene(targetScene);
    }
}