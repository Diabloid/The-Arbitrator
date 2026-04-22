using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("UI Панелі")]
    [SerializeField] private CanvasGroup _gameOverPanelGroup;
    [SerializeField] private GameObject _hudPanel;

    [Header("Налаштування")]
    [SerializeField] private float _fadeDuration = 1f;

    private void Awake()
    {
        // Ініціалізація Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Захист від дублікатів
        }
    }

    private void Start()
    {
        // На старті гри ховаємо панель
        if (_gameOverPanelGroup != null)
        {
            _gameOverPanelGroup.alpha = 0f;
            _gameOverPanelGroup.interactable = false;
            _gameOverPanelGroup.blocksRaycasts = false;
        }
    }

    public void TriggerGameOver()
    {
        if (_hudPanel != null) _hudPanel.SetActive(false);

        Time.timeScale = 0f;

        if (_gameOverPanelGroup != null)
            StartCoroutine(FadeInPanel());
    }

    // Методи для кнопок "Повторити" та "Вийти в головне меню"

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnRetryDeathClicked();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        LoadingScreenManager.Instance.LoadScene(0);
    }

    // Корутина для плавного появлення панелі
    private IEnumerator FadeInPanel()
    {
        float elapsed = 0f;

        _gameOverPanelGroup.blocksRaycasts = true;
        _gameOverPanelGroup.interactable = true;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _gameOverPanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
            yield return null;
        }

        _gameOverPanelGroup.alpha = 1f;
    }
}