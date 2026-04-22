using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("Дані Персонажа")]
    public PlayerStats stats;

    [Header("Налаштування сцени")]
    [SerializeField] private Animator anim;             // Посилання на аніматор
    [SerializeField] private Transform attackPoint;     // Точка атаки
    [SerializeField] public Vector2 attackBoxSize = new Vector2(2f, 1.5f);
    [SerializeField] private LayerMask enemyLayers;     // Шар ворогів
    [SerializeField] private LayerMask obstacleLayer;   // Шар перешкод

    [Header("Ефекти оточення (VFX)")]
    [SerializeField] private GameObject wallSparkPrefab;
    [SerializeField] private LayerMask environmentLayer;

    // Внутрішні змінні
    private float nextAttackTime = 0f;      // Час наступної атаки
    private float lastAirAttackTime = -10f; // Час останньої атаки з повітря
    private bool isPlunging = false;        // Чи зараз ми в стані ривка вниз
    private int comboCount = 0;             // Лічильник комбо
    private int hitCombo = 0;               // Лічильник влучань
    private float lastAttackTime;           // Час останньої атаки
    private int _cachedDamage;
    private float comboResetTime = 1f;

    private PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();

        if (stats == null)
        {
            Debug.LogError("НЕМАЄ СТАТІВ!");
            stats = ScriptableObject.CreateInstance<PlayerStats>();
        }
    }

    void Update()
    {
        if (!player.CanControl || player.IsRolling) return;

        // Таймер скидання комбо
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboCount = 0;
            hitCombo = 0;

            if (player != null)
            {
                player.SetAttackingState(false);
            }
        }

        // Перевірка атаки
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Attack();
                nextAttackTime = Time.time + 1f / stats.attackRate;
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time; // Оновлюємо час останньої атаки

        if (player != null)
        {
            player.AutoTurnToEnemy(enemyLayers);
        }

        int currentDamage = stats.baseDamage; // Змінна для фінального урону

        // 1. Логіка атак на землі
        if (player.IsGrounded())
        {
            player.SetAttackingState(true);

            comboCount++;
            if (comboCount > 3) comboCount = 1;

            _cachedDamage = stats.baseDamage;

            anim.SetTrigger("Attack");
            anim.SetInteger("ComboIndex", comboCount);
        }
        // 2. Логіка атак в повітрі
        else
        {
            // Перевіряємо S + ЛКМ
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                // Перевіряємо кулдаун
                if (Time.time >= lastAirAttackTime + stats.airAttackCooldown)
                {
                    StartPlungeAttack();
                }
                else
                {
                    Debug.Log("Атака перезаряджається!");
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    // Обробка зіткнення при падінні
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isPlunging) return;

        bool isEnemy = (enemyLayers.value & (1 << collision.gameObject.layer)) > 0;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Boss_Platforms") || isEnemy)
        {
            isPlunging = false;
            StartCoroutine(DoPlungeImpact());
        }
    }

    void StartPlungeAttack()
    {
        anim.SetTrigger("AirAttack");

        player.canMove = false;
        player.SetAttackingState(true);

        // Штовхаємо вниз
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.down * stats.plungeSpeed;

        isPlunging = true;
        lastAirAttackTime = Time.time;
        comboCount = 0;
    }

    // Обробка приземлення після ривка вниз
    IEnumerator DoPlungeImpact()
    {
        anim.SetTrigger("Impact");
        isPlunging = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        Debug.Log("BOOM! Приземлення!");
        DealDamage(stats.airAttackDamage, new Vector2(stats.plungeRadius * 2.5f, 1f));

        yield return new WaitForSeconds(stats.impactRecoveryTime);

        player.canMove = true;
        player.SetAttackingState(false);
    }

    void DealDamage(int damage, Vector2 boxSize)
    {
        // Центр удару. Якщо це Plunge - центр в ногах гравця. Якщо звичайна - в AttackPoint
        Vector2 point = isPlunging ? (Vector2)transform.position : (Vector2)attackPoint.position;

        Collider2D wallHit = Physics2D.OverlapBox(point, boxSize, 0f, environmentLayer);
        if (wallHit != null && wallSparkPrefab != null)
        {
            Vector2 sparkPoint = wallHit.ClosestPoint(point);

            Instantiate(wallSparkPrefab, sparkPoint, Quaternion.identity);

            CinemachineShake.Instance?.ShakeCamera(1f, 0.1f);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(point, boxSize, 0f, enemyLayers);

        // 1. Фільтруємо ворогів за стінами
        List<Collider2D> validEnemies = new List<Collider2D>();

        foreach (Collider2D enemy in hitEnemies)
        {
            Vector2 playerPos = (Vector2)transform.position + Vector2.up * 0.5f;
            Vector2 enemyPos = (Vector2)enemy.transform.position + Vector2.up * 0.5f;

            // Пускаємо промінь від гравця до ворога
            RaycastHit2D lineHit = Physics2D.Linecast(playerPos, enemyPos, obstacleLayer);

            if (lineHit.collider != null)
            {
                Debug.DrawLine(playerPos, lineHit.point, Color.red, 2f);
                Debug.Log($"Стіна заблокувала удар по {enemy.name}!");
                continue; // Пропускаємо цього ворога
            }

            // Додаємо в список
            Debug.DrawLine(playerPos, enemyPos, Color.green, 2f);
            validEnemies.Add(enemy);
        }

        // 2. Логіка комбо та критів
        int finalBaseDamage = damage;

        if (validEnemies.Count > 0)
        {
            if (player.isLunging) player.StopLunge();
            hitCombo++;

            int bonusDamage = 0;
            if (hitCombo <= 3)
            {
                bonusDamage = (hitCombo - 1) * 5;
            }
            else
            {
                bonusDamage = 10 + (hitCombo - 3) * 1;
            }

            if (bonusDamage < 0) bonusDamage = 0;
            finalBaseDamage += bonusDamage;

            Debug.Log($"Влучання! HitCombo: {hitCombo}, Бонус: +{bonusDamage}");
        }
        else
        {
            hitCombo = 0;
            Debug.Log("Промах! Комбо скинуто.");
        }

        // 3. Наносимо урон
        foreach (Collider2D enemy in validEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int finalDamage = finalBaseDamage;
                bool isCrit = false;

                if (Random.Range(0f, 100f) <= stats.critChance)
                {
                    finalDamage = Mathf.RoundToInt(finalBaseDamage * stats.critMultiplier);
                    isCrit = true;
                    Debug.Log($"<color=orange>CRITICAL HIT! {finalDamage}</color>");
                }

                damageable.TakeDamage(finalDamage, isCrit);
            }
        }
    }

    public void AnimationEvent_EndAttack()
    {
        player.SetAttackingState(false);
    }

    public void AnimationEvent_Strike()
    {
        DealDamage(_cachedDamage, attackBoxSize);
    }

    public void AnimationEvent_Lunge(float dashForce)
    {
        player.StartLunge(dashForce, 0.2f);
    }

    // Візуалізація зони атаки в редакторі
    void OnDrawGizmosSelected()
    {
        // Малюємо радіус атаки (червоним)
        if (attackPoint == null || stats == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);

        // Малюємо радіус Plunge (синім)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 1f, 0f));
    }
}