using UnityEngine;
using System.Collections;

public class GroundExplosionEffect : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private float preExplosionTime = 2f; // Час затримки перед самим вибухом
    [SerializeField] private int damageAmount = 15;

    [Header("Посилання")]
    [SerializeField] private GameObject warningVisual;
    [SerializeField] private GameObject damageVisual;

    [Header("Звук та Фізика")]
    [SerializeField] private AudioClip explosionSound;
    private AudioSource _audioSource;
    private Collider2D _collider;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider2D>();

        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        // 1. ФАЗА ПОПЕРЕДЖЕННЯ: Вмикаємо маркер, ховаємо вибух
        if (warningVisual != null) warningVisual.SetActive(true);
        if (damageVisual != null) damageVisual.SetActive(false);

        yield return new WaitForSeconds(preExplosionTime);

        // 2. ЧАС ВИТОЧНОГО ВИБУХУ: Ховаємо маркер, вмикаємо візуал вибуху
        if (warningVisual != null) warningVisual.SetActive(false);
        if (damageVisual != null)
        {
            damageVisual.SetActive(true);
        }

        // 3. НАНЕСЕННЯ УРОНУ
        ApplyDamageInArea();

        // 5. ОЧИЩЕННЯ
        yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
    }

    private void ApplyDamageInArea()
    {
        float radius = 1f;
        if (_collider is CircleCollider2D circle) radius = circle.radius;
        else if (_collider is BoxCollider2D box) radius = box.size.x / 2f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                EntityHealth hp = col.GetComponent<EntityHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(damageAmount);
                    Debug.Log("<color=red>БОСФАЙТ: Гравець отримав урон від точкового вибуху!</color>");
                }
            }
        }
    }


    // Гізмо
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 1.0f);

        Collider2D col = GetComponent<Collider2D>();
        if (col is CircleCollider2D circle) Gizmos.DrawWireSphere(transform.position, circle.radius);
    }
}