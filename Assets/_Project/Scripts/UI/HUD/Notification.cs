using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class Notification : MonoBehaviour
{
    public static Notification Instance { get; private set; }

    [Header("UI Елементи")]
    [SerializeField] private CanvasGroup _tutorialPanel;
    [SerializeField] private TMP_Text _tutorialText;
    [SerializeField] private float _fadeSpeed = 2f;

    private bool _hasLearnedToHeal = false;
    private bool _isShowing = false;
    private bool _isAutoFading = false;
    private string _originalTutorialText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (_tutorialText != null) _originalTutorialText = _tutorialText.text;

        if (PlayerPrefs.GetInt("HasLearnedHeal", 0) == 1)
        {
            _hasLearnedToHeal = true;
            if (_tutorialPanel != null) _tutorialPanel.alpha = 0f;
        }
        else
        {
            if (_tutorialPanel != null) _tutorialPanel.alpha = 0f;
        }
    }

    private void Update()
    {
        // Блокуємо закриття через F, якщо це тимчасове авто-повідомлення
        if (_isShowing && !_hasLearnedToHeal && !_isAutoFading && Input.GetKeyDown(KeyCode.F))
        {
            _hasLearnedToHeal = true;
            PlayerPrefs.SetInt("HasLearnedHeal", 1);
            PlayerPrefs.Save();

            StartCoroutine(FadePanel(0f));
        }
    }

    public void ShowTutorialPrompt()
    {
        if (!_hasLearnedToHeal && !_isShowing && _tutorialPanel != null)
        {
            _isShowing = true;
            StartCoroutine(FadePanel(1f));
        }
    }

    // Автоматичне повідомлення
    public void ShowAutoMessage(string newText, float durationSeconds)
    {
        if (_tutorialPanel == null || _tutorialText == null) return;

        StopAllCoroutines();
        StartCoroutine(AutoMessageRoutine(newText, durationSeconds));
    }

    private IEnumerator AutoMessageRoutine(string text, float duration)
    {
        _isAutoFading = true;
        _isShowing = true;

        _tutorialText.text = text;

        yield return StartCoroutine(FadePanel(1f));

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(FadePanel(0f));

        _tutorialText.text = _originalTutorialText;
        _isShowing = false;
        _isAutoFading = false;
    }

    private IEnumerator FadePanel(float targetAlpha)
    {
        while (Mathf.Abs(_tutorialPanel.alpha - targetAlpha) > 0.05f)
        {
            _tutorialPanel.alpha = Mathf.Lerp(_tutorialPanel.alpha, targetAlpha, Time.deltaTime * _fadeSpeed);
            yield return null;
        }
        _tutorialPanel.alpha = targetAlpha;

        if (targetAlpha == 0f && !_isAutoFading) _isShowing = false;
    }

    [ContextMenu("Скинути навчання")]
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("HasLearnedHeal", 0);
        _hasLearnedToHeal = false;
        Debug.Log("Навчання скинуто!");
    }
}