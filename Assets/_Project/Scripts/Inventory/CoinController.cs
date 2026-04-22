using UnityEngine;
using System.Collections;

public class CoinController : MonoBehaviour
{
    [Header("Налаштування Геймплею")]
    [SerializeField] private int amount = 1;
    [SerializeField] private PlayerStats statsToUpdate;

    [Header("Фізика Левітації")]
    [SerializeField] private float floatSpeed = 2f;    // Як швидко коливається вгору-вниз
    [SerializeField] private float floatHeight = 0.1f; // Амплітуда коливання
    [SerializeField] private float hoverHeight = 0.5f; // На яку висоту піднятися від землі
    [SerializeField] private float smoothTime = 3f;    // Наскільки плавно підніматися (чим більше, тим повільніше)

    [Header("Анімація/Ефект")]
    [SerializeField] private Sprite[] animationSprites;
    [SerializeField] private float framesPerSecond = 12f; // Швидкість анімації
    [SerializeField] private GameObject pickupVFXPrefab;

    [Header("Звук")]
    [SerializeField] private AudioClip _pickupSound;

    private Rigidbody2D _rb;
    private CircleCollider2D _cd;
    private SpriteRenderer _sr;

    private bool _isGrounded = false;
    private Vector3 _groundPos;
    private bool _canPickup = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cd = GetComponent<CircleCollider2D>();
        _sr = GetComponent<SpriteRenderer>();

        if (animationSprites.Length > 0)
        {
            StartCoroutine(AnimateSprite());
        }

        // Затримка перед підбором
        StartCoroutine(EnablePickupDelay());
    }

    void Update()
    {
        // Логіка левітації
        if (_isGrounded)
        {
            float newY = _groundPos.y + hoverHeight + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            Vector3 targetPos = new Vector3(_groundPos.x, newY, _groundPos.z);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothTime);
        }
    }

    // Анімація спрайтів
    private IEnumerator AnimateSprite()
    {
        int index = 0;
        float waitTime = 1f / framesPerSecond;

        while (true)
        {
            _sr.sprite = animationSprites[index];

            index = (index + 1) % animationSprites.Length;

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isGrounded && (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                     collision.gameObject.layer == LayerMask.NameToLayer("Boss_Platforms")))
        {
            if (Mathf.Abs(_rb.linearVelocity.y) < 1f)
            {
                StartHovering();
            }
        }

        if (_canPickup && collision.gameObject.CompareTag("Player"))
        {
            Pickup();
        }
    }

    private void StartHovering()
    {
        _isGrounded = true;
        _groundPos = transform.position;

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        _cd.isTrigger = true;

        transform.rotation = Quaternion.identity;
    }

    // Підбір через Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_canPickup && collision.CompareTag("Player"))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (statsToUpdate != null)
        {
            statsToUpdate.AddCurrency(amount);

            if (pickupVFXPrefab != null)
            {
                Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null && _pickupSound != null)
            {
                AudioManager.Instance.PlaySFXRandomPitch(_pickupSound, 0.7f, 0.9f, 1.2f);
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator EnablePickupDelay()
    {
        yield return new WaitForSeconds(0.5f);
        _canPickup = true;
    }
}