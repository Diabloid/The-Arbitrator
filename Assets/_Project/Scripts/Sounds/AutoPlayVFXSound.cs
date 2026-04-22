using UnityEngine;
using System.Collections;

public class AutoPlayVFXSound : MonoBehaviour
{
    [Header("Налаштування звуку")]
    [SerializeField] private AudioClip vfxSound;

    [Tooltip("Гучність ефекту (від 0.0 до 1.0)")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Tooltip("Затримка перед звуком (0 - якщо треба миттєво)")]
    [SerializeField] private float soundDelay = 0f;

    [Header("Варіативність (Пітч)")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.15f;

    private void OnEnable()
    {
        if (vfxSound != null)
        {
            StartCoroutine(PlaySoundRoutine());
        }
    }

    private IEnumerator PlaySoundRoutine()
    {
        // Чекаємо, якщо є затримка
        if (soundDelay > 0f)
        {
            yield return new WaitForSeconds(soundDelay);
        }

        // Граємо звук
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(vfxSound, volume, minPitch, maxPitch);
        }
    }
}