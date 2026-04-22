using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Дані (Stats)")]
    public PlayerStats stats; // Посилання на ScriptableObject зі статами

    [Header("Налаштування Руху")]
    [SerializeField] private float jumpCutMultiplier = 0.6f;    // Наскільки гасити стрибок, якщо відпустити кнопку
    [SerializeField] private float fastFallSpeed = 5f;          // Сила прискорення вниз
    [SerializeField] private float coyoteTime = 0.2f;           // Скільки часу можна стрибати після падіння
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.2f;       // Скільки часу пам'ятати натискання
    private float jumpBufferCounter;

    [Header("Зіткнення")]
    [SerializeField] private LayerMask stopOnLayers;

    [Header("Налаштування Перекату")]
    [SerializeField] private float rollSpeed = 20f;             // Швидкість ривка
    [SerializeField] private float rollDuration = 0.2f;         // Скільки триває ривок (сек)
    [SerializeField] private float rollCooldown = 1f;           // Час перезарядки
    [SerializeField] private TrailRenderer trailRenderer;       // Ефект сліду

    [Header("Бойові стани")]
    public bool isInvincible = false; // Змінна для I-Frames

    [Header("Компоненти")]
    [SerializeField] private Rigidbody2D rb;            // Посилання на Rigidbody2D
    [SerializeField] private Transform groundCheck;     // Точка перевірки землі
    [SerializeField] private LayerMask groundLayer;     // Шар землі
    [SerializeField] private Animator anim;             // Посилання на аніматор
    [SerializeField] private CapsuleCollider2D playerCollider; // Перетягнеш сюди колайдер Гравця
    private Vector2 _defaultColliderSize;
    private Vector2 _defaultColliderOffset;

    // Внутрішні змінні
    private float horizontalInput;
    private bool isGrounded;            // Перевірка чи на землі персонаж
    private bool canRoll = true;        // Чи можна зараз робити ривок?
    private bool isRolling;             // Чи зараз ми в стані ривка?
    private bool isFacingRight = true;  // Напрямок персонажа
    public bool canMove = true;         // Чи може персонаж рухатись?
    public bool isLunging = false;      // Чи ми зараз у стані випаду?
    public bool IsGrounded() => isGrounded; // Дозволяє іншим скриптам питати, чи ми на землі
    private bool isAttacking = false;       // Стан атаки
    private bool _isHurting = false;        // Чи ми зараз в стані отримання удару?
    private bool _isDead = false;           // Чи ми померли?
    private bool _isHealing = false;        // Чи ми зараз в стані лікування?
    private bool _isJumping = false;        // Чи ми зараз стрибнули навмисно?
    private float _defaultGravity;          // Для збереження оригінальної гравітації
    private bool _wasCircleGrounded = false; // Пам'ять про попередній кадр
    [HideInInspector] public Vector3 lastSafePosition; // Пам'ять для води
    private Coroutine _activeLunge;
    public bool CanControl => !_isHurting && !_isDead && !isLunging && !_isHealing && canMove; // Чи можемо ми зараз керувати персонажем?
    public bool IsRolling => isRolling;

    void Start()
    {
        if (rb != null) _defaultGravity = rb.gravityScale;

        if (playerCollider != null)
        {
            _defaultColliderSize = playerCollider.size;
            _defaultColliderOffset = playerCollider.offset;
        }
    }

    void Update()
    {
        if (_isDead) return;

        if (anim != null)
        {
            anim.SetBool("IsRunning", horizontalInput != 0);
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("VerticalVelocity", isGrounded ? 0f : rb.linearVelocity.y);
        }

        if (!CanControl)
        {
            if (_isDead) rb.linearVelocity = Vector2.zero;
            if (isGrounded && !_isHurting) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 1. Якщо ми в стані деша - не даємо керувати персонажем
        if (isRolling) return;

        // 2. Зчитування вводу
        if (!canMove)
        {
            horizontalInput = 0;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        // 3. Логіка розвороту (для правильного напрямку деша)
        if (!isAttacking)
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            else if (horizontalInput < 0 && isFacingRight) Flip();
        }

        // 4. Швидке падіння
        if (!isGrounded && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }

            rb.AddForce(Vector2.down * fastFallSpeed, ForceMode2D.Impulse);
        }

        if (anim != null)
        {
            bool isMoving = horizontalInput != 0;
            anim.SetBool("IsRunning", isMoving);
        }

        // 3.1 Логіка "Койота"
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 3.2 Логіка "Буфера"
        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 3.3 Стрибок
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && canMove && !isAttacking)
        {
            Jump();

            jumpBufferCounter = 0f;
        }

        // 3.4 Короткий стрибок
        if (Input.GetKeyUp(KeyCode.W) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteTimeCounter = 0f;
        }

        // 4. Перекат (Лівий Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll && canMove && !isAttacking && isGrounded)
        {
            StartCoroutine(Roll());
        }

        // 5. Лікування (H)
        if (Input.GetKeyDown(KeyCode.F) && CanControl)
        {
            HealPlayer();
        }
    }

    void FixedUpdate()
    {
        // 1. Сенсор землі
        bool circleGround = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.4f, groundLayer);

        isGrounded = circleGround || hit;
        Vector2 currentSlopeNormal = hit ? hit.normal : Vector2.up;
        float currentSlopeAngle = Vector2.Angle(currentSlopeNormal, Vector2.up);
        bool isHazardUnderFeet = Physics2D.OverlapCircle(groundCheck.position, 0.3f, LayerMask.GetMask("Hazards"));
        float edgeCheckDist = 0.3f;
        bool isLeftSafe = Physics2D.Raycast(groundCheck.position + Vector3.left * edgeCheckDist, Vector2.down, 0.5f, groundLayer);
        bool isRightSafe = Physics2D.Raycast(groundCheck.position + Vector3.right * edgeCheckDist, Vector2.down, 0.5f, groundLayer);

        if (isGrounded && isLeftSafe && isRightSafe && !isHazardUnderFeet && rb.linearVelocity.y <= 0.1f && currentSlopeAngle < 5f)
        {
            lastSafePosition = transform.position + new Vector3(0f, 0.2f, 0f);
        }

        if (circleGround && !_wasCircleGrounded && rb.linearVelocity.y < -0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        _wasCircleGrounded = circleGround;

        if (rb.linearVelocity.y <= 0.1f && isGrounded)
        {
            _isJumping = false;
        }

        // 2. Блокування управління
        if (isLunging) return;
        if (_isDead) return;
        if (_isHurting)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * 10f), rb.linearVelocity.y);
            }
            return;
        }

        // 3. Логіка перекату
        if (isRolling)
        {
            bool rollCircleGround = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
            RaycastHit2D rollHit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.4f, groundLayer);

            if (rollCircleGround || rollHit)
            {
                Vector2 slopeNormal = rollHit ? rollHit.normal : Vector2.up;
                Vector2 slopeDirection = new Vector2(slopeNormal.y, -slopeNormal.x);

                float direction = isFacingRight ? 1f : -1f;
                if (direction < 0) slopeDirection = -slopeDirection;

                rb.linearVelocity = slopeDirection.normalized * rollSpeed;
            }
            return;
        }

        if (!canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float currentSpeed = stats.moveSpeed;
        if (isAttacking) currentSpeed *= 0.2f;

        // 4. Логіка руху та схилів
        if (isGrounded && !_isJumping)
        {
            // Беремо нормаль землі під гравцем
            Vector2 slopeNormal = hit ? hit.normal : Vector2.up;

            if (horizontalInput != 0)
            {
                float stepHeight = 0.3f;
                Vector2 moveDir = new Vector2(horizontalInput > 0 ? 1 : -1, 0);

                // Промінь 1: в ноги
                RaycastHit2D hitLower = Physics2D.Raycast(groundCheck.position, moveDir, 0.3f, groundLayer);

                if (hitLower && hitLower.collider != null)
                {
                    // Промінь 2: в коліно
                    Vector2 kneePos = (Vector2)groundCheck.position + new Vector2(0, stepHeight);
                    RaycastHit2D hitUpper = Physics2D.Raycast(kneePos, moveDir, 0.4f, groundLayer);

                    if (!hitUpper)
                    {
                        slopeNormal = new Vector2(-moveDir.x, 1f).normalized;
                    }
                }
            }

            float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);

            if (slopeAngle > 0.1f && slopeAngle < 89f)
            {
                rb.gravityScale = 0f;

                if (horizontalInput == 0)
                {
                    rb.linearVelocity = Vector2.zero;
                    return;
                }

                Vector2 slopeDirection = new Vector2(slopeNormal.y, -slopeNormal.x);
                if (horizontalInput < 0) slopeDirection = -slopeDirection;

                Vector2 finalVelocity = slopeDirection.normalized * currentSpeed;

                // Магніт
                if (!circleGround) finalVelocity.y -= 5f;

                rb.linearVelocity = finalVelocity;
                return;
            }
            else // Рівна земля
            {
                rb.gravityScale = _defaultGravity;

                if (horizontalInput == 0)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    return;
                }

                if (rb.linearVelocity.y > 0)
                {
                    rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, 0f);
                }
                else
                {
                    rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
                }
                return;
            }
        }

        // 5. Повітря (Падіння та стрибки)
        rb.gravityScale = _defaultGravity;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }
    private void Jump()
    {
        _isJumping = true;
        GetComponent<PlayerAudio>().PlayJump();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * stats.jumpForce, ForceMode2D.Impulse);
    }

    // Coroutine для перекату
    private IEnumerator Roll()
    {
        canRoll = false;   // Вимикаємо можливість перекату
        isRolling = true;  // Вмикаємо стан
        isInvincible = true;
        GetComponent<PlayerAudio>().PlayRoll();

        // Запускаємо анімацію
        if (anim != null) anim.SetBool("IsRolling", true);

        // Зменшуємо висоту вдвічі, щоб прокочуватись під атаками
        if (playerCollider != null)
        {
            playerCollider.size = new Vector2(_defaultColliderSize.x, _defaultColliderSize.y / 2f);
            playerCollider.offset = new Vector2(_defaultColliderOffset.x, _defaultColliderOffset.y - (_defaultColliderSize.y / 4f));
        }

        float direction = isFacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);

        if (trailRenderer != null) trailRenderer.emitting = true;

        // Чекаємо, поки йде анімація перекату
        yield return new WaitForSeconds(rollDuration);

        if (trailRenderer != null) trailRenderer.emitting = false;
        isRolling = false;
        isInvincible = false;

        if (anim != null) anim.SetBool("IsRolling", false);

        if (playerCollider != null)
        {
            playerCollider.size = _defaultColliderSize;
            playerCollider.offset = _defaultColliderOffset;
        }

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    // Розворот персонажа
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // Метод для встановлення стану атаки з інших скриптів
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
    }

    // Coroutine для випаду
    private IEnumerator DoLunge(float dashForce, float duration)
    {
        isLunging = true;
        float timer = 0f;
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        while (timer < duration)
        {
            // 1. Рухаємо
            if (rb != null)
                rb.linearVelocity = new Vector2(facingDir * dashForce, rb.linearVelocity.y);

            // 2. Перевіряємо зіткнення спереду
            float checkDistance = 0.8f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * facingDir, checkDistance, stopOnLayers);

            if (hit.collider != null)
            {
                Debug.Log($"Врізалися в {hit.collider.name}! Гальмуємо.");
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                isLunging = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isLunging = false;
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void StartLunge(float dashForce, float duration)
    {
        if (_activeLunge != null) StopCoroutine(_activeLunge);
        _activeLunge = StartCoroutine(DoLunge(dashForce, duration));
    }

    public void StopLunge()
    {
        if (_activeLunge != null) StopCoroutine(_activeLunge);

        isLunging = false;
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void OnTakeDamage()
    {
        if (_isDead) return;

        isAttacking = false;
        if (isLunging) StopLunge();

        // Запускаємо анімацію
        anim.SetTrigger("Hit");
        _isHurting = true;

        rb.linearVelocity = Vector2.zero;

        StartCoroutine(StopHurt(0.4f));
    }

    public void OnDeath()
    {
        if (_isDead) return;

        isAttacking = false;
        _isDead = true;
        anim.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        GetComponent<Collider2D>().enabled = false;

        Debug.Log("R.I.P.");
    }

    private IEnumerator StopHurt(float duration)
    {
        yield return new WaitForSeconds(duration);
        _isHurting = false;
    }

    private void HealPlayer()
    {
        EntityHealth health = GetComponent<EntityHealth>();

        // Перевірки: чи є здоров'я? чи ми не повні? чи є банки?
        if (health != null && health.currentHealth < health.stats.maxHealth)
        {
            // Пробуємо взяти банку зі статів
            if (stats.UseHealGem())
            {
                StartCoroutine(HealRoutine());
            }
            else
            {
                Debug.Log("Немає зілля!");
            }
        }
    }

    private IEnumerator HealRoutine()
    {
        isAttacking = false;
        _isHealing = true;

        anim.SetTrigger("Heal");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(0.5f);

        // Лікуємо
        GetComponent<EntityHealth>().Heal(stats.healAmount);

        // Відновлюємо рух
        rb.constraints = originalConstraints;

        _isHealing = false;
    }

    // Авто-розворот до найближчого ворога в радіусі 3 метрів
    public void AutoTurnToEnemy(LayerMask enemyLayer)
    {
        // Шукаємо всіх ворогів у радіусі 3 метрів
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 3f, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform closestEnemy = null;
            float minDistance = Mathf.Infinity;

            // Визначаємо найближчого
            foreach (Collider2D enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = enemy.transform;
                }
            }

            // Розворот до найближчого
            if (closestEnemy != null)
            {
                Debug.Log($"[AutoTurn] Бачу: {closestEnemy.name} | Його X: {closestEnemy.position.x} | Мій X: {transform.position.x}");

                if ((closestEnemy.position.x < transform.position.x && isFacingRight) ||
                    (closestEnemy.position.x > transform.position.x && !isFacingRight))
                {
                    Debug.Log("[AutoTurn] Розворот виконано!");
                    Flip();
                }
            }
        }
        else
        {
            // Якщо сканер порожній, ми теж це побачимо
            Debug.Log("[AutoTurn] В радіусі 3 метрів ворогів на шарі Enemy НЕ ЗНАЙДЕНО!");
        }
    }

    public void ResetStateAfterDeath()
    {
        _isDead = false;
        isAttacking = false;

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

        var coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = true;

        if (anim != null) anim.Play("Idle");

        Debug.Log("Стан Айвена скинуто: він знову Dynamic!");
    }

    // Візуалізація перевірки землі в редакторі
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}