using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Посилання на дані")]
    public PlayerStats playerStats;

    [Header("База всіх справ")]
    public MissionData[] allMissionsDatabase;

    [Header("Налаштування Нової Гри")]
    public string newGameScene = "Hub_Castle";
    [Header("UI Збереження")]
    public CanvasGroup saveNotification;

    public static List<string> SessionDeadEnemies = new List<string>();

    private string saveFilePath;
    private int _loadedHealth = -1;
    // Пам'ять для телепортації
    private Vector3 _loadedPosition;
    private bool _shouldLoadPosition = false;
    private Coroutine _saveCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            saveFilePath = Application.persistentDataPath + "/save.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1. Основний метод збереження
    public void SaveGame()
    {
        PlayerData data = new PlayerData();
        data.deadEnemies = SessionDeadEnemies;

        // 1. Пакуємо стати
        data.currency = playerStats.currentCurrency;
        data.healGems = playerStats.currentHealGems;
        data.baseDamage = playerStats.baseDamage;
        data.lawPoints = playerStats.lawPoints;
        data.chaosPoints = playerStats.chaosPoints;

        // 2. Пакуємо поточний квест (якщо він є)
        if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
        {
            data.activeMissionID = MissionManager.Instance.currentMission.missionID;
        }
        else
        {
            data.activeMissionID = "";
        }

        // 3. Пакуємо локацію та координати + ХП
        data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;

            EntityHealth hpScript = player.GetComponent<EntityHealth>();
            data.currentHealth = hpScript != null ? hpScript.currentHealth : 100;
        }
        else
        {
            // Якщо ми зберігаємось в меню
            data.posX = 0f;
            data.posY = 0f;
            data.posZ = 0f;
        }

        // 4. Конвертуємо в JSON і зберігаємо в файл
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        if (saveNotification != null)
        {
            if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
            _saveCoroutine = StartCoroutine(ShowSaveNotification());
        }

        Debug.Log("Гру збережено успішно! Файл: " + saveFilePath);
    }

    // 2. Основний метод завантаження
    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            SessionDeadEnemies = data.deadEnemies != null ? new List<string>(data.deadEnemies) : new List<string>();

            // 1. Відновлюємо стати
            playerStats.currentCurrency = data.currency;
            playerStats.currentHealGems = data.healGems;
            playerStats.baseDamage = data.baseDamage;
            playerStats.lawPoints = data.lawPoints;
            playerStats.chaosPoints = data.chaosPoints;

            // 2. Відновлюємо справу
            if (MissionManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(data.activeMissionID))
                {
                    // Проходимося по нашій базі і шукаємо збіг по ID
                    bool missionFound = false;
                    foreach (var mission in allMissionsDatabase)
                    {
                        if (mission.missionID == data.activeMissionID)
                        {
                            MissionManager.Instance.currentMission = mission;
                            Debug.Log($"Справу відновлено: {mission.missionTitle}");
                            missionFound = true;
                            break;
                        }
                    }

                    if (!missionFound)
                    {
                        Debug.LogWarning($"Справу з ID {data.activeMissionID} не знайдено в базі!");
                        MissionManager.Instance.ClearMission();
                    }
                }
                else
                {
                    MissionManager.Instance.ClearMission();
                }
            }

            // 3. Відновлюємо координати + ХП
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ResetStateAfterDeath();
            }

            _loadedPosition = new Vector3(data.posX, data.posY, data.posZ);
            _loadedHealth = data.currentHealth;
            _shouldLoadPosition = true;

            Debug.Log($"Гру прочитано! Чекаємо завантаження сцени {data.currentScene}, щоб поставити Айвена на {_loadedPosition}");
        }
        else
        {
            Debug.Log("Файл збереження не знайдено! Це перша гра.");
        }
    }

    // 3. Для кнопки "Грати" в головному меню
    public void OnPlayButtonClicked()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            LoadGame();

            Debug.Log($"Завантажуємо збережену гру. Сцена: {data.currentScene}");
            LoadingScreenManager.Instance.LoadScene(data.currentScene);
        }
        else
        {
            Debug.Log("Збережень немає. Починаємо нову гру!");
            LoadingScreenManager.Instance.LoadScene(newGameScene);
        }
    }

    // 4. Для кнопки "Повторити" на екрані смерті
    public void OnRetryDeathClicked()
    {
        OnPlayButtonClicked();
    }

    // Підписуємося на подію "Сцена завантажена"
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Відписуємося
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 5. Телепортуємо Айвена на збережену позицію після завантаження сцени
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (_shouldLoadPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = _loadedPosition;

                EntityHealth hpScript = player.GetComponent<EntityHealth>();
                if (hpScript != null && _loadedHealth > 0)
                {
                    hpScript.LoadSavedHealth(_loadedHealth);
                }

                Debug.Log("Айвена успішно телепортовано на збережену позицію з відновленим ХП!");
            }
            else
            {
                Debug.LogWarning("Сцена завантажилась, але Айвена з тегом 'Player' не знайдено!");
            }

            _shouldLoadPosition = false;
        }
    }



    // Цей тег створює кнопку прямо в меню скрипта в Інспекторі!
    [ContextMenu("Знищити Сейв")]
    public void DeleteSaveFile()
    {
        string pathForDelete = Application.persistentDataPath + "/save.json";

        if (File.Exists(pathForDelete))
        {
            File.Delete(pathForDelete);
            Debug.LogWarning("Файл збереження знищено! Наступний запуск буде як уперше.");
        }
        else
        {
            Debug.Log($"Сейву і так немає за шляхом: {pathForDelete}");
        }

        if (playerStats != null)
        {
            playerStats.currentCurrency = 0;
            playerStats.currentHealGems = 0;
            playerStats.lawPoints = 0;
            playerStats.chaosPoints = 0;
        }

        SessionDeadEnemies.Clear();

        if (Application.isPlaying && MissionManager.Instance != null)
        {
            MissionManager.Instance.ClearMission();
        }
    }

    private IEnumerator ShowSaveNotification()
    {
        if (saveNotification == null) yield break;

        float fadeTime = 0.5f;
        float showTime = 2.3f;
        float timer = 0f;

        // 1. Плавна поява
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            saveNotification.alpha = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }
        saveNotification.alpha = 1f;

        // 2. Висимо на екрані і крутимо годинник
        yield return new WaitForSecondsRealtime(showTime);

        // 3. Плавне зникнення
        timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            saveNotification.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            yield return null;
        }
        saveNotification.alpha = 0f;

        _saveCoroutine = null;
    }
}