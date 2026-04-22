using System;
using UnityEngine;
using UnityEngine.Events;

public class EntityHealth : MonoBehaviour, IDamageable
{
    [Header("Зв'язок зі статами")]
    public CharacterStats stats;

    [Header("Налаштування")]
    public int currentHealth { get; private set; } // Read-only зовні

    [Header("Спец. налаштування")]
    public bool isInvincible = false;
    public bool destroyOnDeath = true;
    public float deathDestroyDelay = 2f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action<int, bool> OnHit;
    public UnityEvent onDeathEvent;

    [Header("Анімації")]
    public Animator anim;

    [Header("Візуал (Тільки спавн ефектів)")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private GameObject blockSparkPrefab;
    [SerializeField] private Vector2 shieldSparkOffset = new Vector2(0.3f, 0.5f);
    private Color originalColor;

    [Header("Звуки (Індивідуальні для кожного)")]
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _deathSound;

    [Header("UI Ворогів (Авто-створення)")]
    public GameObject healthBarPrefab;
    public float healthBarOffset = 1.5f;

    private PlayerController _playerController;

    void Start()
    {
        int maxHealth = stats != null ? stats.maxHealth : 100;
        if (currentHealth == 0)
        {
            currentHealth = maxHealth;
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        _playerController = GetComponent<PlayerController>();

        // Ініціалізація UI ворога (якщо це не гравець)
        if (!gameObject.CompareTag("Player") && healthBarPrefab != null)
        {
            CreateFloatingHealthBar();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage, bool isCritical = false)
    {
        if (isInvincible) return;

        if (_playerController != null && _playerController.isInvincible)
        {
            return;
        }

        EnemyCombat combat = GetComponent<EnemyCombat>();

        // Перевірка блоку
        if (combat != null && combat.isBlocking)
        {
            float facingDir = transform.localScale.x > 0 ? 1 : -1;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            float playerDir = playerObj != null ?
                (playerObj.transform.position.x - transform.position.x) : facingDir;

            if ((facingDir > 0 && playerDir > 0) || (facingDir < 0 && playerDir < 0))
            {
                if (blockSparkPrefab != null)
                {
                    Vector3 sparkPos = transform.position + new Vector3(facingDir * shieldSparkOffset.x, shieldSparkOffset.y, 0f);
                    Instantiate(blockSparkPrefab, sparkPos, Quaternion.identity);
                }

                CinemachineShake.Instance?.ShakeCamera(2f, 0.1f);

                Debug.Log($"<color=cyan>БЛОК!</color> {name} заблокував удар!");
                return;
            }
        }

        currentHealth -= damage;

        // 1. Сповіщаємо систему
        int maxHealth = stats != null ? stats.maxHealth : 100;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHit?.Invoke(damage, isCritical);

        // 2. Локальні ефекти
        if (damagePopupPrefab != null) ShowDamagePopup(damage, isCritical);
        if (spriteRenderer != null) StartCoroutine(FlashColor());

        if (AudioManager.Instance != null && _hitSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_hitSound, 0.7f, 0.85f, 1.15f);
        }

        // Тряка манекена
        HitShake shaker = GetComponentInChildren<HitShake>();
        if (shaker != null)
        {
            shaker.TriggerShake();
        }

        // Тряска камери тільки для Гравця
        if (gameObject.CompareTag("Player"))
        {
            CinemachineShake.Instance?.ShakeCamera(5f, .1f);
        }

        if (currentHealth > 0 && anim != null)
        {
            // Гіперброня для танка
            bool useHyperArmor = false;

            // Якщо це ворог, у нього увімкнена суперброня І він зараз б'є:
            if (combat != null && combat.hasSuperArmor && combat.isAttacking)
            {
                useHyperArmor = true;
                Debug.Log($"<color=orange>ГІПЕРБРОНЯ!</color> {name} отримав урон, але не зупинив атаку!");
            }

            // Запускаємо реакцію на біль ТІЛЬКИ якщо немає гіперброні
            if (!useHyperArmor)
            {
                anim.SetTrigger("Hit");

                if (combat != null)
                {
                    combat.isAttacking = false;
                    combat.EndBlock();
                }

                if (_playerController != null)
                {
                    _playerController.OnTakeDamage();
                }
            }
        }

        Debug.Log($"{name} отримав {damage}. HP: {currentHealth}");

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        onDeathEvent?.Invoke();
        Debug.Log($"{name} загинув.");

        if (AudioManager.Instance != null && _deathSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_deathSound, 1.0f, 0.9f, 1.1f);
        }

        if (!gameObject.CompareTag("Player"))
        {
            EnemyTracker tracker = GetComponent<EnemyTracker>();
            if (tracker != null)
            {
                tracker.MarkAsDead();
            }
        }

        if (anim != null) anim.SetTrigger("Death");

        CloakController cloak = GetComponent<CloakController>();
        if (cloak != null)
        {
            cloak.OnDeathReveal();
        }

        var bt = GetComponent<BehaviorTree.BTree>();
        if (bt != null) bt.enabled = false;

        var combat = GetComponent<EnemyCombat>();
        if (combat != null) combat.enabled = false;

        if (_playerController != null)
        {
            _playerController.OnDeath();

            StartCoroutine(PlayerDeathRoutine());
            return;
        }

        var coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;

        this.enabled = false;

        EnemyLoot loot = GetComponent<EnemyLoot>();
        if (loot != null)
        {
            loot.DropLoot();
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, deathDestroyDelay);
        }
    }

    // Створення UI для ворогів
    void CreateFloatingHealthBar()
    {
        GameObject barObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
        barObj.transform.SetParent(transform);
        barObj.transform.localPosition = new Vector3(0, healthBarOffset, 0);

        // Знаходимо компонент і даємо йому посилання на себе
        HealthBar barScript = barObj.GetComponentInChildren<HealthBar>();
        if (barScript != null)
        {
            // Dependency Injection
            barScript.Initialize(this);
        }
    }

    // ЕФЕКТИ
    void ShowDamagePopup(int damage, bool isCritical)
    {
        Vector3 spawnPos = transform.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 1.5f, 0);
        GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

        DamagePopup popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript != null) popupScript.Setup(damage, isCritical);
    }

    // Метод для завантаження ХП із сейву
    public void LoadSavedHealth(int savedHealth)
    {
        currentHealth = savedHealth;
        int maxHp = stats != null ? stats.maxHealth : 100;
        OnHealthChanged?.Invoke(currentHealth, maxHp);
    }

    public void Heal(int amount)
    {
        if (currentHealth >= stats.maxHealth) return;

        currentHealth += amount;

        if (currentHealth > stats.maxHealth)
        {
            currentHealth = stats.maxHealth;
        }

        // Оновлюємо UI
        int maxHealth = stats != null ? stats.maxHealth : 100;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{name} вилікувався на {amount}. HP: {currentHealth}/{maxHealth}");
    }

    private System.Collections.IEnumerator PlayerDeathRoutine()
    {
        // Даємо гравцю час впасти і програти анімацію смерті
        yield return new WaitForSeconds(1.5f);

        if (GameOverController.Instance != null)
        {
            GameOverController.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameOverController не знайдено на сцені!");
        }
    }

    // Метод для примусового встановлення здоров'я
    public void SetHealth(int amount)
    {
        currentHealth = Mathf.Clamp(amount, 0, stats.maxHealth);
    }

    System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}