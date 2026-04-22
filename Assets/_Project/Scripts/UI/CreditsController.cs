using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [Header("Налаштування Титрів")]
    [SerializeField] private RectTransform creditsText; // Текст, який буде повзти
    [SerializeField] private float scrollSpeed = 50f;   // Швидкість прокрутки
    [SerializeField] private float timeToReturn = 10f;  // Скільки секунд йдуть титри
    [SerializeField] private string hubSceneName = "Hub";

    [Header("Ефекти (опціонально)")]
    [SerializeField] private CanvasGroup screenFadeGroup;

    private bool _isReturning = false;

    private void Start()
    {
        StartCoroutine(AutoReturnRoutine());

        if (screenFadeGroup != null)
        {
            screenFadeGroup.alpha = 1f;
            StartCoroutine(FadeScreen(0f, 1.5f));
        }
    }

    private void Update()
    {
        if (_isReturning) return;

        // 1. Рухаємо текст вгору кожного кадру
        if (creditsText != null)
        {
            creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }

        // 2. Даємо можливість скіпнути титри
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ReturnToHub());
        }
    }

    private IEnumerator AutoReturnRoutine()
    {
        yield return new WaitForSeconds(timeToReturn);

        StartCoroutine(ReturnToHub());
    }

    private IEnumerator ReturnToHub()
    {
        if (_isReturning) yield break;
        _isReturning = true;

        if (screenFadeGroup != null)
        {
            yield return StartCoroutine(FadeScreen(1f, 1f));
        }

        LoadingScreenManager.Instance.LoadScene(hubSceneName);
    }

    // Допоміжна корутина для плавного екрану
    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (screenFadeGroup == null) yield break;

        float startAlpha = screenFadeGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        screenFadeGroup.alpha = targetAlpha;
    }
}