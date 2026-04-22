using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI Елементи")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;      // Повзунок загрузки (опціонально)
    [SerializeField] private TMP_Text progressText;   // Текст відсотків (опціонально)

    [Header("Налаштування")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float minLoadTime = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Виклик за НАЗВОЮ сцени
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneRoutine(sceneName, -1));
    }

    // Виклик за ІНДЕКСОМ сцени
    public void LoadScene(int sceneBuildIndex)
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneRoutine("", sceneBuildIndex));
    }

    // Універсальна корутина
    private IEnumerator LoadSceneRoutine(string sceneName, int sceneIndex)
    {
        if (progressBar != null) progressBar.value = 0f;
        if (progressText != null) progressText.text = "0%";

        // 1. Плавно показуємо чорний екран загрузки
        canvasGroup.blocksRaycasts = true;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        if (progressBar != null) progressBar.value = 0f;
        if (progressText != null) progressText.text = "0%";

        // 2. Асинхронне завантаження (визначаємо, що саме передали)
        AsyncOperation operation;
        if (!string.IsNullOrEmpty(sceneName))
        {
            operation = SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            operation = SceneManager.LoadSceneAsync(sceneIndex);
        }

        operation.allowSceneActivation = false;

        float loadTimer = 0f;

        // 3. Крутимо цикл
        while (!operation.isDone)
        {
            loadTimer += Time.unscaledDeltaTime;
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null) progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * 2f);
            if (progressText != null)
            {
                float displayValue = progressBar != null ? progressBar.value : targetProgress;
                progressText.text = $"{(int)(displayValue * 100)}%";
            }

            if (operation.progress >= 0.9f && loadTimer >= minLoadTime)
            {
                if (progressBar == null || progressBar.value >= 0.99f)
                {
                    operation.allowSceneActivation = true;
                }
            }
            yield return null;
        }

        // 4. Ховаємо екран
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}