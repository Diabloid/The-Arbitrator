using UnityEngine;

public class MagicJumpPad : MonoBehaviour
{
    [Header("Налаштування стрибка")]
    [SerializeField] private float launchVelocity = 16f;
    [SerializeField] private float cooldownTime = 0.5f;

    [Header("Звук")]
    [SerializeField] private AudioClip launchSound;

    private float _lastJumpTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryLaunch(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryLaunch(collision);
    }

    private void TryLaunch(Collider2D collision)
    {
        if (Time.time < _lastJumpTime + cooldownTime) return;

        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Жорстко задаємо швидкість польоту вгору
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, launchVelocity);

                if (AudioManager.Instance != null && launchSound != null)
                {
                    AudioManager.Instance.PlaySFXRandomPitch(launchSound, 0.8f, 0.9f, 1.1f);
                }

                _lastJumpTime = Time.time;
            }
        }
    }
}