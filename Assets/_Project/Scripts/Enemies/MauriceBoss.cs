using UnityEngine;
using System.Collections;

public class MauriceBoss : MonoBehaviour
{
    // Всі фази боса
    public enum BossPhase
    {
        Scout = 0,       // Фаза 1: Розвідка (100% ХП)
        Knockback = 1,   // Фаза 2: Відштовхування (99% - 50% ХП)
        Shield = 2,      // Фаза 3: Щит (50% ХП)
        Stunned = 3,     // Фаза 4: Оглушення (Кристали знищено)
        LastBreath = 4,  // Фаза 5: Останній подих (Після оглушення)
        Defeated = 5     // Чекає на діалог (1% ХП)
    }

    [Header("Левітація Боса")]
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;
    private float _baseYPos;

    [Header("Поточний стан")]
    public BossPhase currentPhase = BossPhase.Scout;
    [SerializeField] private BossHealthUI bossHealthUI;

    [Header("Фаза 1: Розвідка")]
    [SerializeField] private GameObject skeletonPrefab;      // Префаб звичайного скелета
    [SerializeField] private GameObject explosionPrefab;     // Префаб вибуху
    [SerializeField] private AudioClip teleportSound;        // Звук телепортації
    [SerializeField] private Transform[] bossTeleportPoints; // Точки телепорту боса
    [SerializeField] private Transform[] skeletonSpawnPoints;// Точки спавну скелетів
    [SerializeField] private float teleportInterval = 3.5f;  // Як часто стрибає
    [SerializeField] private float explosionInterval = 2.5f; // Як часто спамить вибухи
    [SerializeField] private int initialSkeletonsCount = 2;  // Скільки скелетів на старті
    [SerializeField] private float explosionYOffset = 0.5f;  // Зміщення для вибуху
    [SerializeField] private GameObject teleportVFXPrefab;   // Префаб ефекту телепортації
    [SerializeField] private float startDelay = 1.5f;        // Затримка перед початком бою

    [Header("Фаза 2: Відштовхування")]
    [SerializeField] private float phase2ExplosionInterval = 2.0f;
    [SerializeField] private float knockbackRadius = 3.5f;         // Радіус вибухової хвилі
    [SerializeField] private int knockbackDamage = 15;             // Урон від хвилі
    [SerializeField] private float knockbackForce = 12f;           // Сила відкидання
    [SerializeField] private float knockbackCooldown = 4.0f;       // Захист від спаму відкиданням
    [SerializeField] private GameObject knockbackVFXPrefab;        // Візуал вибухової хвилі навколо боса
    [SerializeField] private int phase2SkeletonsCount = 3;         // Скільки скелетів спавнити після відштовхування
    [SerializeField] private int bossPatienceHits = 3;             // Скільки ударів терпить бос
    [SerializeField] private string flightLayerName = "PlayerFlight"; // Шар для польоту

    [Header("Фаза 3: Щит")]
    [SerializeField] private Transform phase3CenterPoint;    // Точка, де він зупиняється
    [SerializeField] private GameObject shieldVFX;           // Візуал щита
    [SerializeField] private GameObject crystalPrefab;       // Префаб кристала
    [SerializeField] private Transform[] crystalSpawnPoints; // 4 точки для кристалів
    [SerializeField] private float phase3ExplosionInterval = 1.5f;

    [Header("Фаза 4: Оглушення")]
    [SerializeField] private float stunDuration = 6f;        // Скільки секунд бос в стані оглушення
    [SerializeField] private LayerMask groundLayer;          // Вкажемо шар землі
    [SerializeField] private float fallSpeed = 8f;           // Швидкість падіння

    [Header("Фаза 5: Останній подих")]
    [SerializeField] private float phase5ExplosionInterval = 2.5f; // Час між серіями вибухів
    [SerializeField] private int burstExplosionCount = 3;          // Скільки вибухів у серії
    [SerializeField] private float burstExplosionDelay = 0.5f;     // Інтервал між вибухами в серії
    [SerializeField] private float phase5KnockbackForce = 25f;     // Сила фінального мега-відкидання
    [SerializeField] private float phase5KnockbackRadius = 5f;     // Радіус мега-відкидання
    [SerializeField] private GameObject tankSkeletonPrefab;        // Префаб скелета танка
    [SerializeField] private int phase5NormalSkeletons = 3;        // Скільки звичайних скелетів

    [Header("Фінал (Діалог)")]
    [SerializeField] private Transform defeatTeleportPoint;        // Точка, куди він телепортується для діалогу
    [SerializeField] private NPCInteract interactComponent;        // Компонент діалогу
    [SerializeField] private PlayerStats playerStats;              // Посилання на стати гравця для оновлення

    private EntityHealth _health;
    private Transform _playerTransform;

    // Внутрішні змінні
    private float _teleportTimer;
    private float _explosionTimer;
    private bool _isActionLocked = false;
    private Coroutine _currentTeleportCoroutine;
    private bool _isFightStarted = false;
    private SpriteRenderer _bossSprite;
    private float _knockbackTimer = 0f;
    private int _comboHits = 0;
    private float _lastHitTime = 0f;
    private float _stunTimer;
    private bool _isLevitating = true;
    private float _originalBaseY;

    private void Start()
    {
        _baseYPos = transform.position.y;
        _health = GetComponent<EntityHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        if (_health != null)
        {
            _health.OnHit += HandleHit;
        }

        if (interactComponent != null) interactComponent.enabled = false;

        StartCoroutine(StartFightRoutine());
    }

    private void Update()
    {
        if (!_isFightStarted) return;
        if (_health != null && _health.currentHealth <= 0) return;

        if (!_isActionLocked && _isLevitating)
        {
            float newY = _baseYPos + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // МАШИНА СТАНІВ
        switch (currentPhase)
        {
            case BossPhase.Scout:
                Phase1_ScoutLogic();
                break;
            case BossPhase.Knockback:
                Phase2_KnockbackLogic();
                break;
            case BossPhase.Shield:
                Phase3_ShieldLogic();
                break;
            case BossPhase.Stunned:
                Phase4_StunLogic();
                break;
            case BossPhase.LastBreath:
                Phase5_LastBreathLogic();
                break;
            case BossPhase.Defeated:
                FlipTowardsPlayer();
                break;
        }
    }

    private IEnumerator StartFightRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (BossArenaTrigger.Instance != null)
        {
            BossArenaTrigger.Instance.IgnitePhaseSkull((int)currentPhase);
        }

        _teleportTimer = teleportInterval;
        _explosionTimer = explosionInterval;
        SpawnSkeletons(initialSkeletonsCount); ;

        _isFightStarted = true;
    }

    // ЛОГІКА ФАЗ
    #region PHASE 1: SCOUT
    // ФАЗА 1: РОЗВІДКА
    private void SpawnSkeletons(int count)
    {
        Debug.Log($"<color=orange>Моріс призиває {count} міньйонів!</color>");

        // Спочатку анімація
        if (_health.anim != null) _health.anim.SetTrigger("CastSummon");

        System.Collections.Generic.List<Transform> availablePoints = new System.Collections.Generic.List<Transform>(skeletonSpawnPoints);

        for (int i = 0; i < count; i++)
        {
            if (availablePoints.Count > 0 && skeletonPrefab != null)
            {
                int randIndex = Random.Range(0, availablePoints.Count);
                Transform chosenPoint = availablePoints[randIndex];
                availablePoints.RemoveAt(randIndex);

                StartCoroutine(SpawnSkeletonRoutine(chosenPoint.position));
            }
        }
    }

    private IEnumerator SpawnSkeletonRoutine(Vector3 spawnPos)
    {
        // Затримка під анімацію Моріса
        yield return new WaitForSeconds(0.5f);

        if (teleportVFXPrefab != null)
        {
            Vector3 vfxPos = spawnPos - new Vector3(0, 0.8f, 0);
            GameObject vfx = Instantiate(teleportVFXPrefab, vfxPos, Quaternion.identity);
            vfx.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }

        yield return new WaitForSeconds(0.4f);

        if (skeletonPrefab != null)
        {
            Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void Phase1_ScoutLogic()
    {
        if (_isActionLocked) return;

        // 1. Логіка Вибухів
        _explosionTimer -= Time.deltaTime;
        if (_explosionTimer <= 0)
        {
            SpawnExplosionUnderPlayer();
            _explosionTimer = explosionInterval;
        }

        // 2. Логіка Телепорту
        _teleportTimer -= Time.deltaTime;
        if (_teleportTimer <= 0)
        {
            _currentTeleportCoroutine = StartCoroutine(TeleportRoutine());
            _teleportTimer = teleportInterval;
        }
    }

    private void SpawnExplosionUnderPlayer()
    {
        if (_playerTransform == null || explosionPrefab == null) return;

        if (_health.anim != null) _health.anim.SetTrigger("CastAttack");

        StartCoroutine(DelayedExplosionSpawn());
    }

    private IEnumerator DelayedExplosionSpawn()
    {
        // Чекаємо 0.4 секунд (поки Моріс замахується)
        yield return new WaitForSeconds(0.4f);

        // Захист на випадок, якщо гравець кудись зник під час касту
        if (_playerTransform == null) yield break;

        // Тільки тепер беремо свіжі координати Айвена і спавнимо вибух!
        Vector3 spawnPos = _playerTransform.position + new Vector3(0, explosionYOffset, 0);
        Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator TeleportRoutine()
    {
        _isActionLocked = true; // Блокуємо всі дії

        PlayTeleportEffect(transform.position);
        if (_bossSprite != null) _bossSprite.enabled = false;

        yield return new WaitForSeconds(0.5f);

        if (bossTeleportPoints.Length > 0)
        {
            Transform targetPoint = bossTeleportPoints[Random.Range(0, bossTeleportPoints.Length)];
            int attempts = 0;
            while (Vector2.Distance(transform.position, targetPoint.position) < 1f && attempts < 10)
            {
                targetPoint = bossTeleportPoints[Random.Range(0, bossTeleportPoints.Length)];
                attempts++;
            }

            transform.position = targetPoint.position;
            _baseYPos = transform.position.y;
            FlipTowardsPlayer();
        }

        PlayTeleportEffect(transform.position);

        // Чекаємо, поки програється поява
        yield return new WaitForSeconds(0.5f);
        if (_bossSprite != null) _bossSprite.enabled = true;

        _explosionTimer = Mathf.Max(_explosionTimer, 1.0f);
        _teleportTimer = teleportInterval;

        _isActionLocked = false; // Розблоковуємо Моріса
    }
    #endregion

    #region PHASE 2: KNOCKBACK
    private void Phase2_KnockbackLogic()
    {
        // Таймер кулдауну відштовхування йде незалежно ні від чого
        if (_knockbackTimer > 0) _knockbackTimer -= Time.deltaTime;

        if (_isActionLocked) return;

        // 1. Вибухи (стають частішими)
        _explosionTimer -= Time.deltaTime;
        if (_explosionTimer <= 0)
        {
            SpawnExplosionUnderPlayer();
            _explosionTimer = phase2ExplosionInterval;
        }

        // 2. Телепорти
        _teleportTimer -= Time.deltaTime;
        if (_teleportTimer <= 0)
        {
            _currentTeleportCoroutine = StartCoroutine(TeleportRoutine());
            _teleportTimer = teleportInterval;
        }
    }

    private void CastKnockback()
    {
        if (_knockbackTimer > 0) return;

        Debug.Log("<color=cyan>Моріс кастує ВІДШТОВХУВАННЯ!</color>");

        if (_health.anim != null) _health.anim.SetTrigger("CastSummon");
        if (knockbackVFXPrefab != null) Instantiate(knockbackVFXPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, knockbackRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                EntityHealth playerHealth = hit.GetComponent<EntityHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(knockbackDamage);

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                PlayerController pc = hit.GetComponent<PlayerController>();

                if (rb != null && pc != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    float pushDirX = hit.transform.position.x > transform.position.x ? 1f : -1f;
                    Vector2 pushVector = new Vector2(pushDirX, 0f);
                    rb.AddForce(pushVector * knockbackForce, ForceMode2D.Impulse);

                    // Перевірка на FlightLayer і зміна шару для польоту
                    int originalLayer = hit.gameObject.layer;
                    int flightLayerIndex = LayerMask.NameToLayer(flightLayerName);

                    if (flightLayerIndex != -1)
                    {
                        hit.gameObject.layer = flightLayerIndex;
                    }

                    StartCoroutine(StunPlayerRoutine(pc, playerHealth, originalLayer, 1.5f));
                }
            }
        }

        _knockbackTimer = knockbackCooldown;
    }

    // Корутина оглушення гравця
    private IEnumerator StunPlayerRoutine(PlayerController pc, EntityHealth pHealth, int originalLayer, float duration)
    {
        pc.canMove = false;

        // Робимо гравця невразливим до вибухів/ударів під час польоту
        if (pHealth != null) pHealth.isInvincible = true;

        yield return new WaitForSeconds(duration);

        pc.canMove = true;

        // Повертаємо нормальну фізику та можливість отримувати урон
        pc.gameObject.layer = originalLayer;
        if (pHealth != null) pHealth.isInvincible = false;
    }
    #endregion

    #region PHASE 3: SHIELD
    private IEnumerator Phase3SetupRoutine()
    {
        _isActionLocked = true;

        // 1. Телепорт у центр
        PlayTeleportEffect(transform.position);
        if (_bossSprite != null) _bossSprite.enabled = false;

        yield return new WaitForSeconds(0.5f);

        if (phase3CenterPoint != null)
        {
            transform.position = phase3CenterPoint.position;
            _baseYPos = transform.position.y;
            FlipTowardsPlayer();
        }

        PlayTeleportEffect(transform.position);
        yield return new WaitForSeconds(0.5f);

        if (_bossSprite != null) _bossSprite.enabled = true;

        Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
        if (rb2 != null) rb2.constraints = RigidbodyConstraints2D.FreezeAll;

        // 2. Вмикаємо щит
        if (shieldVFX != null) shieldVFX.SetActive(true);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Відкидаємо гравця в сторону
                    float pushDirX = hit.transform.position.x > transform.position.x ? 1f : -1f;
                    Vector2 pushVector = new Vector2(pushDirX, 0.5f).normalized;

                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(pushVector * 15f, ForceMode2D.Impulse);
                }
            }
        }

        // 3. Анімація касту та спавн кристалів
        Debug.Log("<color=cyan>Моріс піднімає АБСОЛЮТНИЙ ЩИТ!</color>");
        if (_health.anim != null) _health.anim.SetTrigger("CastCrystal");

        foreach (Transform pt in crystalSpawnPoints)
        {
            PlayTeleportEffect(pt.position);
            if (crystalPrefab != null) Instantiate(crystalPrefab, pt.position, Quaternion.identity);
        }

        _explosionTimer = phase3ExplosionInterval;
        _isActionLocked = false;
    }

    private bool AreCrystalsDead()
    {
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsSortMode.None);
        int aliveCrystals = 0;

        foreach (var entity in allEntities)
        {
            if (entity.gameObject.name.Contains("Crystal") && entity.currentHealth > 0)
            {
                aliveCrystals++;
            }
        }
        return aliveCrystals == 0;
    }

    private void Phase3_ShieldLogic()
    {
        if (_isActionLocked) return;

        // 1. Шалений спам вибухами
        _explosionTimer -= Time.deltaTime;
        if (_explosionTimer <= 0)
        {
            SpawnExplosionUnderPlayer();
            _explosionTimer = phase3ExplosionInterval;
        }

        // 2. Перевіряємо умову перемоги в цій фазі
        if (AreCrystalsDead())
        {
            // Якщо всі 4 кристали розбиті - щит падає
            if (shieldVFX != null) shieldVFX.SetActive(false);
            _health.isInvincible = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            ChangePhase(BossPhase.Stunned);
        }
    }
    #endregion

    #region PHASE 4: STUNNED
    private void Phase4_StunLogic()
    {
        _stunTimer -= Time.deltaTime;

        if (_stunTimer <= 0)
        {
            ChangePhase(BossPhase.LastBreath);
        }
    }

    // Стан
    private IEnumerator FallToGroundRoutine()
    {
        _isLevitating = false;
        _originalBaseY = _baseYPos;
        Collider2D col = GetComponent<Collider2D>();

        // Опускаємо боса, поки він не торкнеться шару землі
        while (col != null && !col.IsTouchingLayers(groundLayer))
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            yield return null;
        }
    }
    #endregion

    #region PHASE 5: LAST BREATH
    // ПІДЙОМ ТА СТАРТ ФІНАЛУ
    private IEnumerator RiseToAirRoutine()
    {
        while (transform.position.y < _originalBaseY)
        {
            transform.Translate(Vector3.up * fallSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, _originalBaseY, transform.position.z);
        _baseYPos = _originalBaseY;
        _isLevitating = true;

        // СТАРТ ФАЗИ 5
        CastMegaKnockback();
        SpawnFinalArmy();
        _explosionTimer = phase5ExplosionInterval;
        _teleportTimer = teleportInterval;
    }

    private void Phase5_LastBreathLogic()
    {
        if (_isActionLocked) return;

        // 1. Телепорт (Повертається звичний режим стрибків)
        _teleportTimer -= Time.deltaTime;
        if (_teleportTimer <= 0)
        {
            _currentTeleportCoroutine = StartCoroutine(TeleportRoutine());
            _teleportTimer = teleportInterval;
        }

        // 2. Серійні вибухи (Потрійні)
        _explosionTimer -= Time.deltaTime;
        if (_explosionTimer <= 0)
        {
            StartCoroutine(BurstExplosionRoutine());
            _explosionTimer = phase5ExplosionInterval;
        }
    }

    // Мега-Відштовхування (на початку 5 фази)
    private void CastMegaKnockback()
    {
        Debug.Log("<color=red>Моріс лютує! МЕГА-ВІДШТОВХУВАННЯ!</color>");

        if (_health.anim != null) _health.anim.SetTrigger("CastSummon");

        if (knockbackVFXPrefab != null)
        {
            GameObject vfx = Instantiate(knockbackVFXPrefab, transform.position, Quaternion.identity);
            vfx.transform.localScale = new Vector3(2f, 2f, 1f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, phase5KnockbackRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                EntityHealth playerHealth = hit.GetComponent<EntityHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(knockbackDamage * 2); // Подвійний урон!

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                PlayerController pc = hit.GetComponent<PlayerController>();

                if (rb != null && pc != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    float pushDirX = hit.transform.position.x > transform.position.x ? 1f : -1f;
                    Vector2 pushVector = new Vector2(pushDirX, 0f);
                    rb.AddForce(pushVector * phase5KnockbackForce, ForceMode2D.Impulse);

                    int originalLayer = hit.gameObject.layer;

                    StartCoroutine(StunPlayerRoutine(pc, playerHealth, originalLayer, 2f));
                }
            }
        }
    }

    // Спавн Фінальної Армії
    private void SpawnFinalArmy()
    {
        SpawnSkeletons(phase5NormalSkeletons);

        if (tankSkeletonPrefab != null && skeletonSpawnPoints.Length > 0)
        {
            Transform spawnPoint = skeletonSpawnPoints[Random.Range(0, skeletonSpawnPoints.Length)];
            StartCoroutine(SpawnSingleEntityRoutine(tankSkeletonPrefab, spawnPoint.position));
        }
    }

    private IEnumerator SpawnSingleEntityRoutine(GameObject prefab, Vector3 spawnPos)
    {
        yield return new WaitForSeconds(0.5f);
        if (teleportVFXPrefab != null) Instantiate(teleportVFXPrefab, spawnPos - new Vector3(0, 0.8f, 0), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        if (prefab != null) Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    // Потрійний вибух
    private IEnumerator BurstExplosionRoutine()
    {
        if (_health.anim != null) _health.anim.SetTrigger("CastAttack");
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < burstExplosionCount; i++)
        {
            if (_playerTransform == null || currentPhase == BossPhase.Defeated) break;

            Vector3 spawnPos = _playerTransform.position + new Vector3(0, explosionYOffset, 0);
            Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(burstExplosionDelay);
        }
    }


    #endregion

    // Допоміжний метод: фліп на гравця
    private void FlipTowardsPlayer()
    {
        if (_playerTransform == null) return;

        float facingDir = _playerTransform.position.x > transform.position.x ? 1 : -1;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * facingDir;
        transform.localScale = newScale;
    }

    // Допоміжний метод: вбити всіх міньйонів
    private void KillAllSkeletons()
    {
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsSortMode.None);
        foreach (var entity in allEntities)
        {
            if ((entity.gameObject.name.Contains("Skeleton_01_Minion") || entity.gameObject.name.Contains("Skeleton_02_Minion")) && entity.currentHealth > 0)

            {
                entity.TakeDamage(9999);
            }
        }
    }

    // Допоміжний метод: перевірити, чи всі міньйони мертві
    private bool AreSkeletonsDead()
    {
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsSortMode.None);
        int aliveSkeletons = 0;

        foreach (var entity in allEntities)
        {
            if ((entity.gameObject.name.Contains("Skeleton_01_Minion") || entity.gameObject.name.Contains("Skeleton_02_Minion")) && entity.currentHealth > 0)
            {
                aliveSkeletons++;
            }
        }

        return aliveSkeletons == 0;
    }

    // Допоміжний метод: граємо ефект телепортації
    private void PlayTeleportEffect(Vector3 spawnPosition)
    {
        if (teleportVFXPrefab != null)
        {
            Instantiate(teleportVFXPrefab, spawnPosition, Quaternion.identity);
        }
    }


    // ОБРОБКА УРОНУ ТА ЗМІНА ФАЗ

    private void HandleHit(int damage, bool isCritical)
    {
        if (_health == null) return;

        // ТРЮК З 99%
        if (currentPhase == BossPhase.Scout)
        {
            _health.SetHealth(Mathf.RoundToInt(_health.stats.maxHealth * 0.99f));

            ChangePhase(BossPhase.Knockback);
            return;
        }

        float hpPercent = (float)_health.currentHealth / _health.stats.maxHealth;

        if (currentPhase == BossPhase.Knockback)
        {
            if (hpPercent <= 0.5f)
            {
                _health.SetHealth(Mathf.RoundToInt(_health.stats.maxHealth * 0.5f));

                ChangePhase(BossPhase.Shield);
                _health.isInvincible = true;
            }
            else
            {
                // СИСТЕМА ТЕРПІННЯ (По ударах)

                // Якщо Айвен відбіг і не бив більше 2 секунд - бос заспокоюється
                if (Time.time - _lastHitTime > 2.0f) _comboHits = 0;

                _lastHitTime = Time.time;
                _comboHits++;

                // Якщо бос не на кулдауні відкидання
                if (_knockbackTimer <= 0)
                {
                    // Ситуація 1: Скелетів немає (Паніка)
                    if (AreSkeletonsDead())
                    {
                        CastKnockback();
                        SpawnSkeletons(phase2SkeletonsCount);
                        _comboHits = 0;
                    }
                    // Ситуація 2: Скелети є, але Айвен нахабніє (наніс 3 удари)
                    else if (_comboHits >= bossPatienceHits)
                    {
                        CastKnockback();
                        _comboHits = 0;
                    }
                }
            }
        }

        // ТРЮК З 1%
        if (currentPhase == BossPhase.LastBreath)
        {
            if (hpPercent <= 0.02f || _health.currentHealth <= 1)
            {
                int onePercent = Mathf.Max(1, Mathf.RoundToInt(_health.stats.maxHealth * 0.01f));
                _health.SetHealth(onePercent);
                _health.isInvincible = true;

                ChangePhase(BossPhase.Defeated);
            }
        }
    }

    private void ChangePhase(BossPhase newPhase)
    {
        if (currentPhase == newPhase) return;

        if (_currentTeleportCoroutine != null) StopCoroutine(_currentTeleportCoroutine);
        _isActionLocked = false;

        currentPhase = newPhase;
        Debug.Log($"<color=orange>Моріс переходить у фазу: {newPhase}!</color>");

        if (BossArenaTrigger.Instance != null)
        {
            BossArenaTrigger.Instance.IgnitePhaseSkull((int)newPhase);
        }

        if (newPhase == BossPhase.Knockback)
        {
            _explosionTimer = phase2ExplosionInterval;
            CastKnockback();
            if (BossArenaTrigger.Instance != null) BossArenaTrigger.Instance.FlashChandeliers();
            if (bossHealthUI != null) bossHealthUI.ShowUI();
        }
        else if (newPhase == BossPhase.Shield)
        {
            StartCoroutine(Phase3SetupRoutine());
        }
        else if (newPhase == BossPhase.Stunned)
        {
            _stunTimer = stunDuration;

            KillAllSkeletons();

            StartCoroutine(FallToGroundRoutine());
        }
        else if (newPhase == BossPhase.LastBreath)
        {
            StartCoroutine(RiseToAirRoutine());
        }
        else if (newPhase == BossPhase.Defeated)
        {
            _isActionLocked = true;
            _isLevitating = false;

            KillAllSkeletons();

            if (BossArenaTrigger.Instance != null) BossArenaTrigger.Instance.RestoreArenaAfterFight();

            if (defeatTeleportPoint != null)
            {
                transform.position = defeatTeleportPoint.position;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if (interactComponent != null) interactComponent.enabled = true;
        }
    }

    // МЕТОДИ ДЛЯ ФІНАЛЬНОГО ДІАЛОГУ (ВИБОРИ)

    // ВИБІР 1: ПОМИЛУВАТИ
    public void SpareBoss()
    {
        Debug.Log("<color=green>Гравець обрав: ПОМИЛУВАТИ.</color>");
        if (playerStats != null) playerStats.AddChaosPoints(1);
        StartCoroutine(SpareRoutine());
    }

    private IEnumerator SpareRoutine()
    {
        // 1. Граємо ефект телепорту (ніби він тікає)
        PlayTeleportEffect(transform.position);
        
        // 2. Ховаємо Моріса і вимикаємо діалог
        if (_bossSprite != null) _bossSprite.enabled = false;
        if (interactComponent != null) interactComponent.enabled = false;

        // Ховаємо ХП бар боса
        if (bossHealthUI != null) bossHealthUI.HideUI();

        yield return new WaitForSeconds(0.5f);

        // 3. Відкриваємо стіни арени
        if (BossArenaTrigger.Instance != null) BossArenaTrigger.Instance.UnlockArenaWalls();
       
        Destroy(gameObject, 1f); 
    }

    // ВИБІР 2: ВБИТИ
    public void ExecuteBoss()
    {
        Debug.Log("<color=red>Гравець обрав: ВБИТИ.</color>");
        if (playerStats != null) playerStats.AddLawPoints(1);

        // 1. Знімаємо сюжетне безсмертя і наносимо фатальний удар
        _health.isInvincible = false;
        _health.TakeDamage(9999);

        if (bossHealthUI != null) bossHealthUI.HideUI();

        // 2. Вимикаємо компонент діалогу, щоб не можна було говорити з трупом
        if (interactComponent != null) interactComponent.enabled = false;

        // 3. Відкриваємо стіни арени
        if (BossArenaTrigger.Instance != null) BossArenaTrigger.Instance.UnlockArenaWalls();
    }

    // СИСТЕМА ДІАЛОГІВ (ПІДПИСКА НА ПОДІЇ)

    private void OnEnable()
    {
        DialogueManager.OnDialogueActionTriggered += HandleDialogueAction;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueActionTriggered -= HandleDialogueAction;
    }

    private void HandleDialogueAction(string actionId)
    {
        // Якщо це наші кодові слова - запускаємо відповідні кінцівки
        if (actionId == "SpareBoss")
        {
            SpareBoss();
        }
        else if (actionId == "KillBoss")
        {
            ExecuteBoss();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHit -= HandleHit;
        }
    }
}