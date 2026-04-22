using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    [Header("Унікальний ідентифікатор")]
    public string enemyID;

    void Start()
    {
        // При старті сцени моб питає: "Я є в списку мертвих?"
        if (SaveManager.SessionDeadEnemies.Contains(enemyID))
        {
            gameObject.SetActive(false);
        }
    }

    public void MarkAsDead()
    {
        if (!SaveManager.SessionDeadEnemies.Contains(enemyID))
        {
            SaveManager.SessionDeadEnemies.Add(enemyID);
            Debug.Log($"Моб {enemyID} записаний у книгу мертвих.");
        }
    }
}