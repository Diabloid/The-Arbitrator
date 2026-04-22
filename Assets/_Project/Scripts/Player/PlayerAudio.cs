using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Звуки ходьби, стрибків та перекату")]
    [SerializeField] private AudioClip _footstepSound;
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _landSound;
    [SerializeField] private AudioClip _rollSound;

    [Header("Звуки атак")]
    [SerializeField] private AudioClip _lightSwingSound; // Звичайний замах
    [SerializeField] private AudioClip _thrustSound;     // Випад
    [SerializeField] private AudioClip _heavySwingSound; // Сильний удар
    [SerializeField] private AudioClip _airAttackSound;  // Атака з повітря

    [Header("Звуки стану Айвена")]
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _healSound;

    // Кроки
    public void PlayFootstep()
    {
        if (AudioManager.Instance != null && _footstepSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_footstepSound, 0.4f, 0.85f, 1.15f);
        }
    }

    // 1. Для звичайної атаки
    public void PlayLightSwing()
    {
        if (AudioManager.Instance != null && _lightSwingSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_lightSwingSound, 0.8f, 0.9f, 1.1f);
        }
    }

    // 2. Для випаду
    public void PlayThrust()
    {
        if (AudioManager.Instance != null && _thrustSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_thrustSound, 0.8f, 0.95f, 1.05f);
        }
    }

    // 3. Для сильного удару
    public void PlayHeavySwing()
    {
        if (AudioManager.Instance != null && _heavySwingSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_heavySwingSound, 0.8f, 0.8f, 0.95f);
        }
    }

    // 4. Для удару з повітря
    public void PlayAirAttack()
    {
        Debug.Log("ВИКЛИК ЗВУКУ ПОВІТРЯНОЇ АТАКИ! Час: " + Time.time);

        if (AudioManager.Instance != null && _airAttackSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_airAttackSound, 0.8f, 0.9f, 1.2f);
        }
    }

    // Стрибок
    public void PlayJump()
    {
        if (AudioManager.Instance != null && _jumpSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_jumpSound, 0.2f, 0.95f, 1.1f);
        }
    }

    // Приземлення
    public void PlayLand()
    {
        if (AudioManager.Instance != null && _landSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_landSound, 0.8f, 0.8f, 0.95f);
        }
    }

    // Перекат
    public void PlayRoll()
    {
        if (AudioManager.Instance != null && _rollSound != null)
        {
            // Перекат має звучати динамічно, тому рандомний пітч тут ідеально підходить
            AudioManager.Instance.PlaySFXRandomPitch(_rollSound, 0.7f, 0.9f, 1.1f);
        }
    }

    // Удар по Айвену
    public void PlayHit()
    {
        if (AudioManager.Instance != null && _hitSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_hitSound, 0.9f, 0.9f, 1.1f);
        }
    }

    // Смерть
    public void PlayDeath()
    {
        if (AudioManager.Instance != null && _deathSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_deathSound, 1f, 0.95f, 1.05f);
        }
    }

    // Лікування
    public void PlayHeal()
    {
        if (AudioManager.Instance != null && _healSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_healSound, 0.6f, 0.95f, 1.1f);
        }
    }
}