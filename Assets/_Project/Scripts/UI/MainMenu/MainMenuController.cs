using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Панелі (Canvas Groups)")]
    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private CanvasGroup _settingsPanel;
    [SerializeField] private CanvasGroup _blackScreen;

    [Header("Час анімацій")]
    [SerializeField] private float _startupFadeDuration = 1.5f;
    [SerializeField] private float _panelTransitionTime = 0.3f;
    [SerializeField] private float _actionDelay = 1f;

    [Header("Кнопки")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        if (_blackScreen != null)
        {
            _blackScreen.alpha = 1f;
            _blackScreen.blocksRaycasts = true;

            StartCoroutine(FadeCanvasGroup(_blackScreen, 1f, 0f, _startupFadeDuration, true));
        }

        SetupPanelInstant(_mainPanel, true);
        SetupPanelInstant(_settingsPanel, false);
    }

    // Методи для кнопок "Налаштування" та "Назад"

    public void OpenSettings()
    {
        StartCoroutine(TransitionPanels(_mainPanel, _settingsPanel));
    }

    public void CloseSettings()
    {
        StartCoroutine(TransitionPanels(_settingsPanel, _mainPanel));
    }

    // Методи для кнопок "Грати" та "Вихід"

    public void PlayGame()
    {
        if (_playButton != null) _playButton.interactable = false;

        if (_blackScreen != null)
        {
            _blackScreen.blocksRaycasts = true;
            StartCoroutine(FadeCanvasGroup(_blackScreen, 0f, 1f, _actionDelay, false));
        }

        StartCoroutine(PlayGameRoutine());
    }

    public void QuitGame()
    {
        if (_quitButton != null) _quitButton.interactable = false;
        StartCoroutine(QuitGameRoutine());
    }

    // Корутини для виконання дій після анімації чорного екрану

    private IEnumerator PlayGameRoutine()
    {
        yield return new WaitForSeconds(_actionDelay);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnPlayButtonClicked();
        }
        else
        {
            LoadingScreenManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private IEnumerator QuitGameRoutine()
    {
        yield return new WaitForSeconds(_actionDelay);
        Application.Quit();
    }

    private IEnumerator TransitionPanels(CanvasGroup panelOut, CanvasGroup panelIn)
    {
        panelOut.blocksRaycasts = false;

        yield return StartCoroutine(FadeCanvasGroup(panelOut, 1f, 0f, _panelTransitionTime, false));

        yield return StartCoroutine(FadeCanvasGroup(panelIn, 0f, 1f, _panelTransitionTime, false));

        panelIn.blocksRaycasts = true;
    }

    // Універсальний математичний метод для зміни Alpha
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration, bool disableRaycastOnFinish)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = endAlpha;

        if (disableRaycastOnFinish && endAlpha == 0f)
        {
            cg.blocksRaycasts = false;
        }
    }

    // Допоміжний метод для миттєвого налаштування панелі
    private void SetupPanelInstant(CanvasGroup cg, bool isVisible)
    {
        cg.alpha = isVisible ? 1f : 0f;
        cg.blocksRaycasts = isVisible;
    }
}