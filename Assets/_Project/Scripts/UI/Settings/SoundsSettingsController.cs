using UnityEngine;
using System.Collections;

public class SoundsSettingsController : MonoBehaviour
{
    [Header("Внутрішні панелі (Canvas Groups)")]
    [SerializeField] private CanvasGroup _mainSettingsGroup;
    [SerializeField] private CanvasGroup _deleteWarningGroup;

    [Header("Налаштування")]
    [SerializeField] private float _fadeDuration = 0.2f;

    private void OnEnable()
    {
        SetupPanelInstant(_mainSettingsGroup, true);
        SetupPanelInstant(_deleteWarningGroup, false);
    }

    // Логіка попередження
    public void OpenDeleteWarning()
    {
        if (_mainSettingsGroup != null) _mainSettingsGroup.interactable = false;

        StartCoroutine(FadeWarningPanel(_deleteWarningGroup, 0f, 1f, true));
    }

    public void CloseDeleteWarning()
    {
        if (_mainSettingsGroup != null) _mainSettingsGroup.interactable = true;

        StartCoroutine(FadeWarningPanel(_deleteWarningGroup, 1f, 0f, false));
    }

    public void ConfirmDeleteSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        CloseDeleteWarning();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            Time.timeScale = 1f; // Відновлюємо час, якщо була пауза
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // Метод для плавного показу/приховування панелі попередження
    private IEnumerator FadeWarningPanel(CanvasGroup cg, float startAlpha, float endAlpha, bool isOpening)
    {
        if (cg == null) yield break;

        cg.blocksRaycasts = isOpening;

        float elapsed = 0f;
        // Використовуємо unscaledDeltaTime, щоб працювало на Паузі
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / _fadeDuration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    private void SetupPanelInstant(CanvasGroup cg, bool isVisible)
    {
        if (cg == null) return;
        cg.alpha = isVisible ? 1f : 0f;
        cg.blocksRaycasts = isVisible;
        cg.interactable = true;
    }
}