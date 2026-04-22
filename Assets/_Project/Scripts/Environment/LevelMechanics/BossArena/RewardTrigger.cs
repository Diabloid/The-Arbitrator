using UnityEngine;

public class BossRewardTrigger : MonoBehaviour
{
    [Header("Налаштування нагороди")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float messageDuration = 4f;

    [Header("Текст повідомлення")]
    [TextArea(2, 3)]
    [SerializeField] private string rewardMessage = "Отримано нагороду від Імперії.";

    private bool _hasGivenReward = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Якщо це гравець і ми ще не видавали нагороду
        if (!_hasGivenReward && collision.CompareTag("Player"))
        {
            _hasGivenReward = true;

            int finalGoldReward = 0;

            // 1. Беремо золото з місії і видаляємо її
            if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
            {
                finalGoldReward = MissionManager.Instance.currentMission.baseGoldReward;

                Debug.Log($"Справу '{MissionManager.Instance.currentMission.missionTitle}' виконано! Нагорода: {finalGoldReward}");

                MissionManager.Instance.ClearMission();
            }

            // 2. Додаємо золото гравцю
            if (playerStats != null && finalGoldReward > 0)
            {
                playerStats.AddCurrency(finalGoldReward);
            }

            // 3. Відображаємо повідомлення про нагороду
            if (Notification.Instance != null)
            {
                Notification.Instance.ShowAutoMessage(rewardMessage, messageDuration);
            }
            else
            {
                Debug.LogWarning("HealTutorial не знайдено на сцені!");
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
        }
    }
}