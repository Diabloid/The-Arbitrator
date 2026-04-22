using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public static event System.Action<string> OnDialogueActionTriggered;

    [Header("UI Панелі")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Система Виборів")]
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choicesContainer;

    [Header("Налаштування друку")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Звук друку тексту")]
    [SerializeField] private AudioClip dialogueBlipSound;
    [SerializeField][Range(1, 5)] private int blipFrequency = 2;

    [Header("Налаштування Камери (Cinemachine)")]
    [SerializeField] private CinemachineCamera dialogueCamera;

    private bool _isDialogueActive = false;
    private Coroutine _typingCoroutine;
    private string _currentFullLine;
    private bool _isTyping = false;
    private bool _isWaitingForInput = false;

    private float _inputCooldownTimer;
    private PlayerController _playerRef;

    private DialogueNode _currentNode;
    private List<GameObject> _activeChoiceButtons = new List<GameObject>();
    public bool IsDialogueActive => _isDialogueActive;
    private string _pendingActionId = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        dialogueUI.SetActive(false);

        if (dialogueCamera != null)
        {
            dialogueCamera.Priority = 0;
        }
    }

    public void StartDialogue(DialogueNode startingNode, Transform npcTransform)
    {
        if (_isDialogueActive) return;
        _pendingActionId = "";

        _isDialogueActive = true;
        dialogueUI.SetActive(true);

        _inputCooldownTimer = Time.time + 0.2f;
        FreezePlayer(true);

        if (dialogueCamera != null && npcTransform != null)
        {
            dialogueCamera.Follow = npcTransform;
            dialogueCamera.Priority = 20;
        }

        DisplayNode(startingNode);
    }

    private void DisplayNode(DialogueNode node)
    {
        _currentNode = node;

        speakerNameText.text = node.speakerName;
        speakerNameText.color = node.speakerColor;

        speakerNameText.outlineColor = node.speakerOutlineColor;
        speakerNameText.outlineWidth = node.speakerOutlineWidth;

        _currentFullLine = node.npcText;

        ClearChoiceButtons();
        choicesContainer.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(true);

        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeLine(_currentFullLine));
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;
        dialogueText.text = "";

        int letterCount = 0;

        AudioClip clipToPlay = (_currentNode.voiceSound != null) ? _currentNode.voiceSound : dialogueBlipSound;
        float basePitch = (_currentNode.voiceSound != null) ? _currentNode.voicePitch : 1f;

        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            letterCount++;

            if (c != ' ' && letterCount % blipFrequency == 0)
            {
                if (AudioManager.Instance != null && clipToPlay != null)
                {
                    // Граємо звук із відхиленням +- 0.1 від базового пітчу персонажа
                    AudioManager.Instance.PlaySFXRandomPitch(clipToPlay, 0.2f, basePitch - 0.1f, basePitch + 0.1f);
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
        _isWaitingForInput = true;
    }

    private void Update()
    {
        if (!_isDialogueActive) return;
        if (Time.time < _inputCooldownTimer) return;

        // Логіка інпуту (E або Пробіл)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            if (_isTyping)
            {
                // 1. Якщо ще друкується - скіпаємо текст і одразу чекаємо дій
                StopCoroutine(_typingCoroutine);
                dialogueText.text = _currentFullLine;
                _isTyping = false;
                _isWaitingForInput = true;
            }
            else if (_isWaitingForInput)
            {
                // 2. Якщо вже прочитали і тиснемо Е ще раз
                _isWaitingForInput = false;

                if (_currentNode.choices != null && _currentNode.choices.Length > 0)
                {
                    ShowChoices();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    // Логіка показу виборів гравця
    private void ShowChoices()
    {
        dialogueText.gameObject.SetActive(false);
        choicesContainer.gameObject.SetActive(true);

        foreach (DialogueChoice choice in _currentNode.choices)
        {
            // 1. Створюємо кнопку з префабу
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
            _activeChoiceButtons.Add(btnObj);

            // 2. Вписуємо текст гравця
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = choice.choiceText;

            // 3. Вішаємо подію
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(choice.actionId))
                    {
                        _pendingActionId = choice.actionId;
                    }

                    OnChoiceSelected(choice.nextNode);
                });
            }
        }
    
    }

    // Що відбувається, коли гравець натискає на кнопку відповіді
    private void OnChoiceSelected(DialogueNode nextNode)
    {
        if (nextNode != null)
        {
            DisplayNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    // Знищувач кнопок
    private void ClearChoiceButtons()
    {
        foreach (GameObject btn in _activeChoiceButtons)
        {
            Destroy(btn);
        }
        _activeChoiceButtons.Clear();
    }

    // Завершення діалогу
    public void EndDialogue()
    {
        _isDialogueActive = false;
        dialogueUI.SetActive(false);

        FreezePlayer(false);

        if (dialogueCamera != null)
        {
            dialogueCamera.Priority = 0;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (!string.IsNullOrEmpty(_pendingActionId))
        {
            OnDialogueActionTriggered?.Invoke(_pendingActionId);
            _pendingActionId = "";
        }
    }

    // Блокування руху гравця під час діалогу
    private void FreezePlayer(bool freeze)
    {
        if (_playerRef == null)
        {
            _playerRef = FindAnyObjectByType<PlayerController>();
        }

        if (_playerRef != null)
        {
            _playerRef.canMove = !freeze;
        }
    }
}