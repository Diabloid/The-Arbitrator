using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Зв'язок зі статами")]
    public EnemyStats enemyStats;

    [Header("Префаби (Що спавнити)")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject HealGemPrefab;

    [Header("Фізика")]
    [SerializeField] private float dropForce = 5f;

    public void DropLoot()
    {
        if (enemyStats == null) return;

        // 1. Монети
        if (coinPrefab != null)
        {
            int amount = Random.Range(enemyStats.minCoins, enemyStats.maxCoins + 1);
            for (int i = 0; i < amount; i++) SpawnItem(coinPrefab);
        }

        // 2. Зілля
        if (HealGemPrefab != null && Random.Range(0f, 100f) <= enemyStats.healGemDropChance)
        {
            SpawnItem(HealGemPrefab);
        }
    }

    private void SpawnItem(GameObject itemPrefab)
    {
        GameObject item = Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            float randomX = Random.Range(-1f, 1f);
            float randomY = Random.Range(1f, 2f);
            Vector2 forceDir = new Vector2(randomX, randomY).normalized;
            rb.AddForce(forceDir * dropForce, ForceMode2D.Impulse);
        }
    }
}