using UnityEngine;

public class SlimeAmbushManager : MonoBehaviour
{
    [Header("Збереження")]
    [Tooltip("Назви унікально, наприклад: Ceiling_Ambush_01")]
    [SerializeField] private string ambushID;

    [Header("Налаштування Засідки")]
    [SerializeField] private EntityHealth triggerSlimeHealth;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private Transform[] dropPoints;

    private void Start()
    {
        // 1. Якщо засідку вже проходили — вимикаємо цей скрипт
        if (!string.IsNullOrEmpty(ambushID) && SaveManager.SessionDeadEnemies.Contains(ambushID))
        {
            Debug.Log($"<color=green>Засідка {ambushID} вже пройдена. Вимикаємо.</color>");
            gameObject.SetActive(false);
            return;
        }

        // Підписуємося на смерть тригер-слиза
        if (triggerSlimeHealth != null)
        {
            triggerSlimeHealth.OnDeath += TriggerAmbush;
        }
    }

    private void TriggerAmbush()
    {
        Debug.Log("<color=yellow>Тригер-слиз помер! Слизняковий дощ починається!</color>");

        // 2. Записуємо саму засідку в Книгу мертвих, щоб не повторювалась
        if (!string.IsNullOrEmpty(ambushID) && !SaveManager.SessionDeadEnemies.Contains(ambushID))
        {
            SaveManager.SessionDeadEnemies.Add(ambushID);
        }

        int index = 1;
        foreach (Transform dropPoint in dropPoints)
        {
            if (dropPoint != null && slimePrefab != null)
            {
                GameObject slime = Instantiate(slimePrefab, dropPoint.position, Quaternion.identity);

                // 🟢 3. ВИДАЄМО ПАСПОРТИ!
                // Робимо так, щоб у кожного слиза був унікальний ID (наприклад Ceiling_Ambush_01_1)
                EnemyTracker tracker = slime.GetComponent<EnemyTracker>();
                if (tracker != null)
                {
                    tracker.enemyID = ambushID + "_" + index;
                }
                index++;
            }
        }

        // Відписуємося від події, щоб не було помилок
        if (triggerSlimeHealth != null)
        {
            triggerSlimeHealth.OnDeath -= TriggerAmbush;
        }
    }

    // 🟢 4. Захист від помилок пам'яті
    private void OnDestroy()
    {
        if (triggerSlimeHealth != null)
        {
            triggerSlimeHealth.OnDeath -= TriggerAmbush;
        }
    }
}