using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class BossArenaTrigger : MonoBehaviour
{
    public static BossArenaTrigger Instance { get; private set; }

    [Header("Гравець")]
    [SerializeField] private PlayerController player;

    [Header("Стіни (Локдаун)")]
    [SerializeField] private Transform leftWallSpriteBody;
    [SerializeField] private Transform rightWallSpriteBody;
    [SerializeField] private GameObject leftWallCollider;
    [SerializeField] private GameObject rightWallCollider;

    [SerializeField] private float wallRaiseAmount = 4f;
    [SerializeField] private float wallRaiseDuration = 4f;
    [SerializeField] private float wallShakeIntensity = 0.1f;

    [Header("Ефекти")]
    [SerializeField] private ParticleSystem leftWallParticles;
    [SerializeField] private ParticleSystem rightWallParticles;
    [SerializeField] private AudioClip lockdownSound;
    [SerializeField] private AudioClip bossFightMusic;

    [Header("Камера та Бос")]
    [SerializeField] private CinemachineCamera bossCamera;
    [SerializeField] private GameObject bossObject;

    [Header("UI (Інтерфейс)")]
    [SerializeField] private CanvasGroup playerHUDGroup;
    [SerializeField] private float hudFadeDuration = 0.3f;

    [Header("Поява Боса (Телепорт)")]
    [SerializeField] private GameObject bossTeleportVFX;
    [SerializeField] private float teleportDuration = 1f;

    [Header("Елементи Арени")]
    [SerializeField] private GameObject[] jumpPads;

    [Header("Світло (Драматизм)")]
    [SerializeField] private Light2D[] windowLights;
    [SerializeField] private Light2D[] chandelierLights;
    [SerializeField] private Color chandelierFightColor = Color.red;

    [Header("Черепи (Індикатори Фаз)")]
    [SerializeField] private Animator[] phaseSkulls;

    private bool _hasTriggered = false;
    private Vector3 _leftWallStartPos;
    private Vector3 _rightWallStartPos;

    // Пам'ять для оригінального світла
    private float[] _windowStartIntensities;
    private Color[] _chandelierStartColors;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerController>();

        if (leftWallSpriteBody) _leftWallStartPos = leftWallSpriteBody.localPosition;
        if (rightWallSpriteBody) _rightWallStartPos = rightWallSpriteBody.localPosition;

        if (leftWallSpriteBody) leftWallSpriteBody.localPosition = _leftWallStartPos - new Vector3(0, wallRaiseAmount, 0);
        if (rightWallSpriteBody) rightWallSpriteBody.localPosition = _rightWallStartPos - new Vector3(0, wallRaiseAmount, 0);

        if (leftWallCollider) leftWallCollider.GetComponent<Collider2D>().enabled = false;
        if (rightWallCollider) rightWallCollider.GetComponent<Collider2D>().enabled = false;

        if (bossCamera) bossCamera.Priority = 0;
        if (bossObject) bossObject.SetActive(false);
        if (bossTeleportVFX) bossTeleportVFX.SetActive(false);

        foreach (GameObject pad in jumpPads)
        {
            if (pad != null) pad.SetActive(false);
        }

        _windowStartIntensities = new float[windowLights.Length];
        for (int i = 0; i < windowLights.Length; i++)
        {
            if (windowLights[i] != null) _windowStartIntensities[i] = windowLights[i].intensity;
        }

        _chandelierStartColors = new Color[chandelierLights.Length];
        for (int i = 0; i < chandelierLights.Length; i++)
        {
            if (chandelierLights[i] != null) _chandelierStartColors[i] = chandelierLights[i].color;
        }

        foreach (Animator skullAnim in phaseSkulls)
        {
            if (skullAnim != null)
            {
                skullAnim.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;
            StartCoroutine(ArenaLockdownSequence());
        }
    }

    private IEnumerator ArenaLockdownSequence()
    {
        if (playerHUDGroup != null) StartCoroutine(FadeHUDRoutine(0f, hudFadeDuration));

        if (player != null)
        {
            player.canMove = false;
            player.enabled = false;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
            Animator playerAnim = player.GetComponentInChildren<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetFloat("Speed", 0f);
                playerAnim.SetBool("IsRunning", false);
                playerAnim.Play("Player_Idle");
            }
        }

        if (bossCamera) bossCamera.Priority = 30;
        if (leftWallParticles) leftWallParticles.Play();
        if (rightWallParticles) rightWallParticles.Play();

        if (AudioManager.Instance != null && bossFightMusic != null)
        {
            AudioManager.Instance.PlayMusic(bossFightMusic, true);
        }

        if (AudioManager.Instance != null && lockdownSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(lockdownSound, 1f, 0.5f, 0.7f);
        }

        Vector3 leftTargetLocal = _leftWallStartPos;
        Vector3 rightTargetLocal = _rightWallStartPos;
        Vector3 leftStartLocal = _leftWallStartPos - new Vector3(0, wallRaiseAmount, 0);
        Vector3 rightStartLocal = _rightWallStartPos - new Vector3(0, wallRaiseAmount, 0);

        if (leftWallCollider) leftWallCollider.GetComponent<Collider2D>().enabled = true;
        if (rightWallCollider) rightWallCollider.GetComponent<Collider2D>().enabled = true;

        float progress = 0f;
        bool teleportStarted = false;

        // ОСНОВНИЙ ЦИКЛ (Стіни + Світло)
        while (progress < 1f)
        {
            progress += Time.deltaTime / wallRaiseDuration;

            for (int i = 0; i < windowLights.Length; i++)
            {
                if (windowLights[i] != null)
                    windowLights[i].intensity = Mathf.Lerp(_windowStartIntensities[i], 0f, progress);
            }

            for (int i = 0; i < chandelierLights.Length; i++)
            {
                if (chandelierLights[i] != null)
                    chandelierLights[i].color = Color.Lerp(_chandelierStartColors[i], chandelierFightColor, progress);
            }

            if (progress >= 0.75f && !teleportStarted)
            {
                teleportStarted = true;
                if (bossTeleportVFX) bossTeleportVFX.SetActive(true);
            }

            Vector3 shake = new Vector3(Random.Range(-wallShakeIntensity, wallShakeIntensity), 0, 0);
            if (leftWallSpriteBody) leftWallSpriteBody.localPosition = Vector3.Lerp(leftStartLocal, leftTargetLocal, progress) + shake;
            if (rightWallSpriteBody) rightWallSpriteBody.localPosition = Vector3.Lerp(rightStartLocal, rightTargetLocal, progress) + shake;

            yield return null;
        }

        if (leftWallSpriteBody) leftWallSpriteBody.localPosition = _leftWallStartPos;
        if (rightWallSpriteBody) rightWallSpriteBody.localPosition = _rightWallStartPos;
        if (leftWallParticles) leftWallParticles.Stop();
        if (rightWallParticles) rightWallParticles.Stop();

        yield return new WaitForSeconds(teleportDuration * 0.1f);
        if (bossObject) bossObject.SetActive(true);
        yield return new WaitForSeconds(teleportDuration * 0.1f);
        if (bossTeleportVFX) bossTeleportVFX.SetActive(false);

        foreach (GameObject pad in jumpPads)
        {
            if (pad != null) pad.SetActive(true);
        }

        if (player != null)
        {
            player.enabled = true;
            player.canMove = true;
        }

        if (playerHUDGroup != null) StartCoroutine(FadeHUDRoutine(1f, hudFadeDuration));
    }

    // ПУБЛІЧНІ МЕТОДИ (для боса)

    // Бос буде викликати це при кожній новій фазі
    public void IgnitePhaseSkull(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < phaseSkulls.Length)
        {
            Animator skullAnim = phaseSkulls[phaseIndex];
            if (skullAnim != null)
            {
                skullAnim.gameObject.SetActive(true);

                skullAnim.SetTrigger("Ignite");

                Light2D skullLight = skullAnim.GetComponent<Light2D>();
                if (skullLight != null) skullLight.enabled = true;

                if (skullAnim.transform.parent != null)
                {
                    SpriteRenderer parentSprite = skullAnim.transform.parent.GetComponent<SpriteRenderer>();
                    if (parentSprite != null)
                    {
                        Color c = parentSprite.color;
                        c.a = 0f;
                        parentSprite.color = c;
                    }
                }
            }
        }
    }

    // Бос викличе це перед смертю
    public void RestoreArenaAfterFight()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPreviousMusic();
        }

        StartCoroutine(RevertArenaSequence());
    }

    private IEnumerator RevertArenaSequence()
    {
        float duration = 3f;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / duration;

            for (int i = 0; i < windowLights.Length; i++)
            {
                if (windowLights[i] != null)
                    windowLights[i].intensity = Mathf.Lerp(0f, _windowStartIntensities[i], progress);
            }

            for (int i = 0; i < chandelierLights.Length; i++)
            {
                if (chandelierLights[i] != null)
                    chandelierLights[i].color = Color.Lerp(chandelierFightColor, _chandelierStartColors[i], progress);
            }

            yield return null;
        }

        // Гасимо черепи
        foreach (Animator skull in phaseSkulls)
        {
            if (skull != null)
            {
                if (skull.transform.parent != null)
                {
                    SpriteRenderer parentSprite = skull.transform.parent.GetComponent<SpriteRenderer>();
                    if (parentSprite != null)
                    {
                        Color c = parentSprite.color;
                        c.a = 1f;
                        parentSprite.color = c;
                    }
                }

                skull.gameObject.SetActive(false);
            }
        }

        if (bossCamera) bossCamera.Priority = 0;
    }

    // Універсальна корутина для плавного зникнення/появи HUD
    private IEnumerator FadeHUDRoutine(float targetAlpha, float duration)
    {
        if (playerHUDGroup == null) yield break;

        float startAlpha = playerHUDGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerHUDGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        playerHUDGroup.alpha = targetAlpha;
    }

    // МЕТОД ДЛЯ СПАЛАХУ ЛЮСТР
    public void FlashChandeliers()
    {
        StartCoroutine(FlashChandeliersRoutine());
    }

    private IEnumerator FlashChandeliersRoutine()
    {
        float flashDuration = 1.5f;
        float halfDuration = flashDuration / 2f;
        float flashIntensityMultiplier = 2.5f;

        float[] baseIntensities = new float[chandelierLights.Length];
        for (int i = 0; i < chandelierLights.Length; i++)
        {
            if (chandelierLights[i] != null) baseIntensities[i] = chandelierLights[i].intensity;
        }

        // 1. Різко спалахуємо
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < chandelierLights.Length; i++)
            {
                if (chandelierLights[i] != null)
                    chandelierLights[i].intensity = Mathf.Lerp(baseIntensities[i], baseIntensities[i] * flashIntensityMultiplier, elapsed / halfDuration);
            }
            yield return null;
        }

        // 2. Плавно згасаємо до норми
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < chandelierLights.Length; i++)
            {
                if (chandelierLights[i] != null)
                    chandelierLights[i].intensity = Mathf.Lerp(baseIntensities[i] * flashIntensityMultiplier, baseIntensities[i], elapsed / halfDuration);
            }
            yield return null;
        }

        // Точно повертаємо до початкового значення
        for (int i = 0; i < chandelierLights.Length; i++)
        {
            if (chandelierLights[i] != null) chandelierLights[i].intensity = baseIntensities[i];
        }
    }

    // МЕТОД ДЛЯ ВІДКРИТТЯ АРЕНИ
    public void UnlockArenaWalls()
    {
        StartCoroutine(UnlockWallsRoutine());
    }

    private IEnumerator UnlockWallsRoutine()
    {
        float duration = 2f;
        float progress = 0f;

        Vector3 leftStartLocal = _leftWallStartPos;
        Vector3 rightStartLocal = _rightWallStartPos;

        Vector3 leftTargetLocal = _leftWallStartPos - new Vector3(0, wallRaiseAmount, 0);
        Vector3 rightTargetLocal = _rightWallStartPos - new Vector3(0, wallRaiseAmount, 0);

        if (AudioManager.Instance != null && lockdownSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(lockdownSound, 1f, 0.6f, 0.7f);
        }

        // Плавне опускання з тряскою
        while (progress < 1f)
        {
            progress += Time.deltaTime / duration;

            Vector3 shake = new Vector3(Random.Range(-wallShakeIntensity, wallShakeIntensity), 0, 0);
            if (leftWallSpriteBody) leftWallSpriteBody.localPosition = Vector3.Lerp(leftStartLocal, leftTargetLocal, progress) + shake;
            if (rightWallSpriteBody) rightWallSpriteBody.localPosition = Vector3.Lerp(rightStartLocal, rightTargetLocal, progress) + shake;

            yield return null;
        }

        // Фіксуємо рівно на місці і вимикаємо колайдери
        if (leftWallSpriteBody) leftWallSpriteBody.localPosition = leftTargetLocal;
        if (rightWallSpriteBody) rightWallSpriteBody.localPosition = rightTargetLocal;

        if (leftWallCollider) leftWallCollider.GetComponent<Collider2D>().enabled = false;
        if (rightWallCollider) rightWallCollider.GetComponent<Collider2D>().enabled = false;
    }
}