using UnityEngine;

public class WaterStepper : MonoBehaviour
{
    [Header("Налаштування кроків")]
    [SerializeField] private float stepInterval = 0.3f;
    [SerializeField] private float splashForce = -5f;

    [Header("Посилання")]
    [SerializeField] private Collider2D rippleCollider;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Перевірка Води")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float checkRadius = 0.2f;

    [Header("Звуки")]
    [SerializeField] private AudioClip _waterStepSound;

    private Rigidbody2D _rippleRb;
    private float _timer;

    private void Start()
    {
        if (rippleCollider != null)
        {
            _rippleRb = rippleCollider.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (playerRb == null) return;
        bool isMoving = Mathf.Abs(playerRb.linearVelocity.x) > 0.5f;
        bool isInWater = Physics2D.OverlapCircle(transform.position, checkRadius, waterLayer);

        // Бризки та звук працюють ТІЛЬКИ якщо ми йдемо І ми у воді
        if (isMoving && isInWater)
        {
            _timer += Time.deltaTime;

            if (_timer >= stepInterval)
            {
                GenerateRipple();
                _timer = 0f;
            }
        }
        else
        {
            _timer = 0f;
        }
    }

    private void GenerateRipple()
    {
        if (rippleCollider != null)
        {
            if (_rippleRb != null)
            {
                _rippleRb.linearVelocity = new Vector2(0f, splashForce);
            }

            rippleCollider.enabled = false;
            rippleCollider.enabled = true;
        }

        if (AudioManager.Instance != null && _waterStepSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_waterStepSound, 0.2f, 0.85f, 1.15f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}