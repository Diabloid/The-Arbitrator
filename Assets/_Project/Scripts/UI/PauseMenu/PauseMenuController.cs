using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Панелі Паузи (Canvas Groups)")]
    [SerializeField] private CanvasGroup _pauseScreenGroup;  // Загальний екран (з темним фоном)
    [SerializeField] private CanvasGroup _mainButtonsGroup;  // Тільки кнопки паузи
    [SerializeField] private CanvasGroup _settingsGroup;     // Панель налаштувань
    [SerializeField] private CanvasGroup _exitWarningGroup;  // Попередження про вихід
    [SerializeField] private CanvasGroup _missionInfoGroup;  // Панель з інформацією про справу

    [Header("Інформація про Справу")]
    [SerializeField] private TMP_Text _missionTitleText;
    [SerializeField] private TMP_Text _missionTargetText;
    [SerializeField] private TMP_Text _missionLocationText;
    [SerializeField] private TMP_Text _missionRewardText;

    [Header("Час анімацій")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private float _panelTransitionTime = 0.2f;

    public static bool IsPaused { get; private set; } = false;

    private void Start()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SetupPanelInstant(_pauseScreenGroup, false);
        SetupPanelInstant(_mainButtonsGroup, false);
        SetupPanelInstant(_settingsGroup, false);
        SetupPanelInstant(_exitWarningGroup, false);
        SetupPanelInstant(_missionInfoGroup, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isBoardOpen = BoardManager.Instance != null && BoardManager.Instance.IsBoardOpen;
            bool isDialogueOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;
            bool boardJustClosed = (Time.unscaledTime - BoardManager.LastCloseTime) < 0.1f;

            if (isBoardOpen || isDialogueOpen || boardJustClosed) return;

            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        SetupPanelInstant(_mainButtonsGroup, true);
        SetupPanelInstant(_missionInfoGroup, true);
        SetupPanelInstant(_settingsGroup, false);
        SetupPanelInstant(_exitWarningGroup, false);

        UpdateMissionInfo();

        StartCoroutine(FadeCanvasGroup(_pauseScreenGroup, 0f, 1f, _fadeDuration, true));
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        StartCoroutine(FadeCanvasGroup(_pauseScreenGroup, 1f, 0f, _fadeDuration, false));
    }

    // Метод для оновлення справи
    private void UpdateMissionInfo()
    {
        if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
        {
            MissionData mission = MissionManager.Instance.currentMission;

            if (_missionTitleText != null) _missionTitleText.text = mission.missionTitle;
            if (_missionTargetText != null) _missionTargetText.text = $"Ціль: {mission.targetName}";
            if (_missionLocationText != null) _missionLocationText.text = $"Локація: {mission.locationName}";

            if (_missionRewardText != null) _missionRewardText.text = $"Нагорода: {mission.baseGoldReward}";
        }
        else
        {
            if (_missionTitleText != null) _missionTitleText.text = "Справа відсутня";
            if (_missionTargetText != null) _missionTargetText.text = "Ціль: ---";
            if (_missionLocationText != null) _missionLocationText.text = "Локація: ---";
            if (_missionRewardText != null) _missionRewardText.text = "Нагорода: ---";
        }
    }

    // Переходи
    public void OpenSettings()
    {
        StartCoroutine(TransitionPanels(_mainButtonsGroup, _settingsGroup));
        if (_missionInfoGroup != null) StartCoroutine(FadeCanvasGroup(_missionInfoGroup, 1f, 0f, _panelTransitionTime, false));
    }

    public void CloseSettings()
    {
        StartCoroutine(TransitionPanels(_settingsGroup, _mainButtonsGroup));
        if (_missionInfoGroup != null) StartCoroutine(FadeCanvasGroup(_missionInfoGroup, 0f, 1f, _panelTransitionTime, true));
    }

    public void OpenExitWarning()
    {
        StartCoroutine(TransitionPanels(_mainButtonsGroup, _exitWarningGroup));
        if (_missionInfoGroup != null) StartCoroutine(FadeCanvasGroup(_missionInfoGroup, 1f, 0f, _panelTransitionTime, false));
    }

    public void CloseExitWarning()
    {
        StartCoroutine(TransitionPanels(_exitWarningGroup, _mainButtonsGroup));
        if (_missionInfoGroup != null) StartCoroutine(FadeCanvasGroup(_missionInfoGroup, 0f, 1f, _panelTransitionTime, true));
    }

    // Логіка кнопок
    public void SaveGameManual()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("Гра успішно збережена з меню паузи!");
        }
    }

    public void ConfirmExitToMainMenu()
    {
        Debug.Log("Вихід у меню. Незбережений прогрес втрачено.");
        Time.timeScale = 1f;
        LoadingScreenManager.Instance.LoadScene(0);
    }

    private IEnumerator TransitionPanels(CanvasGroup panelOut, CanvasGroup panelIn)
    {
        panelOut.blocksRaycasts = false;

        yield return StartCoroutine(FadeCanvasGroup(panelOut, 1f, 0f, _panelTransitionTime, false));

        yield return StartCoroutine(FadeCanvasGroup(panelIn, 0f, 1f, _panelTransitionTime, true));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration, bool enableRaycastsAtEnd)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
        cg.blocksRaycasts = enableRaycastsAtEnd;
    }

    private void SetupPanelInstant(CanvasGroup cg, bool isVisible)
    {
        if (cg == null) return;
        cg.alpha = isVisible ? 1f : 0f;
        cg.blocksRaycasts = isVisible;
    }
}