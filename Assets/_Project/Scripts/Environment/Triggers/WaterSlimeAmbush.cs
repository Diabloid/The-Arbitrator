using UnityEngine;
using System.Collections;

public class WaterSlimeAmbush : MonoBehaviour
{
    [Header("Збереження (Унікальний ID)")]
    [SerializeField] private string ambushID;

    [Header("Налаштування Засідки")]
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private Transform waterSpawnPoint;
    [SerializeField] private Vector2 jumpForce = new Vector2(-5f, 10f);

    private bool _hasTriggered = false;

    private void Start()
    {
        if (string.IsNullOrEmpty(ambushID)) return;

        if (SaveManager.SessionDeadEnemies.Contains(ambushID))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;

            // Одразу вимикаємо колайдер тригера, щоб він не спрацював двічі
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            StartCoroutine(SpawnAndJumpRoutine());
        }
    }

    private IEnumerator SpawnAndJumpRoutine()
    {
        GameObject slime = Instantiate(slimePrefab, waterSpawnPoint.position, Quaternion.identity);

        EnemyTracker tracker = slime.GetComponent<EnemyTracker>();
        if (tracker != null)
        {
            tracker.enemyID = ambushID;
        }

        var bTree = slime.GetComponent<BehaviorTree.BTree>();
        if (bTree != null) bTree.enabled = false;

        Rigidbody2D rb = slime.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(1.2f);

        if (slime != null && bTree != null)
        {
            bTree.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waterSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(waterSpawnPoint.position, 0.3f);
            Gizmos.DrawRay(waterSpawnPoint.position, jumpForce.normalized * 2f);
        }
    }
}