using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 🔥 Додаємо роботу з фокусом UI

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("UI Елементи")]
    [SerializeField] private GameObject boardUIWindow;
    [SerializeField] private Transform missionListContainer;
    [SerializeField] private GameObject missionButtonPrefab;

    [Header("Панель деталей")]
    [SerializeField] private TMP_Text missionTitleText;
    [SerializeField] private TMP_Text missionDescText;
    [SerializeField] private TMP_Text arbitratorNotesText;
    [SerializeField] private Button acceptButton;

    [Header("База даних Мандатів")]
    public List<MissionData> availableMissions; // Список справ, які зараз доступні

    private MissionData _selectedMission; // Місія, яку гравець зараз розглядає
    private PlayerController _playerRef; // Щоб заморожувати гравця
    public bool IsBoardOpen => boardUIWindow != null && boardUIWindow.activeSelf;
    public static float LastCloseTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        boardUIWindow.SetActive(false);
        ClearDetailsPanel();
    }

    private void Update()
    {
        if (IsBoardOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBoard();
        }
    }

    // Метод для відкриття
    public void OpenBoard()
    {
        if (boardUIWindow.activeSelf) return;

        boardUIWindow.SetActive(true);
        FreezePlayer(true);
        ClearDetailsPanel();
        PopulateMissionList();
    }

    // Метод для закриття
    public void CloseBoard()
    {
        LastCloseTime = Time.unscaledTime;
        boardUIWindow.SetActive(false);
        FreezePlayer(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Генеруємо кнопки зліва
    private void PopulateMissionList()
    {
        foreach (Transform child in missionListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (MissionData mission in availableMissions)
        {
            GameObject btnObj = Instantiate(missionButtonPrefab, missionListContainer);
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();

            if (btnText != null) btnText.text = mission.missionTitle;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => ShowMissionDetails(mission));
            }
        }
    }

    // Виводимо текст вироку справа
    private void ShowMissionDetails(MissionData mission)
    {
        _selectedMission = mission;

        missionTitleText.text = mission.missionTitle;
        missionDescText.text = $"{mission.missionDescription}\n\nЦіль: {mission.targetName}\nЛокація: {mission.locationName}\nНагорода: {mission.baseGoldReward} монет.";

        if (arbitratorNotesText != null)
        {
            // Якщо нотатки є, виводимо їх. Якщо ні - залишаємо пусте місце
            if (!string.IsNullOrEmpty(mission.arbitratorNotes))
                arbitratorNotesText.text = $"Нотатки Вел: {mission.arbitratorNotes}";
            else
                arbitratorNotesText.text = "";
        }

        acceptButton.gameObject.SetActive(true);
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(ConfirmMandate);
    }

    private void ClearDetailsPanel()
    {
        missionTitleText.text = "Оберіть справу";
        missionDescText.text = "Натисніть на справу зі списку зліва, щоб ознайомитись із справою.";
        if (arbitratorNotesText != null) arbitratorNotesText.text = "";
        acceptButton.gameObject.SetActive(false);
    }

    // Гравець тисне "Підписати"
    private void ConfirmMandate()
    {
        if (_selectedMission != null && MissionManager.Instance != null)
        {
            // Передаємо місію в наш глобальний менеджер!
            MissionManager.Instance.AcceptMission(_selectedMission);

            CloseBoard(); // Закриваємо вікно
        }
    }

    // Метод для видалення всіх місій
    public void ClearAllMissions()
    {
        if (availableMissions != null)
        {
            availableMissions.Clear();
        }
    }

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