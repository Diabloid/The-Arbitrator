using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    [Header("Налаштування Шипів")]
    public int spikeDamage = 15;
    [Tooltip("Сила, з якою Айвена відкине від шипів")]
    public float knockbackForce = 12f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DealDamageAndKnockback(collision.gameObject);
        }
    }

    private void DealDamageAndKnockback(GameObject player)
    {
        EntityHealth health = player.GetComponent<EntityHealth>();
        PlayerController controller = player.GetComponent<PlayerController>();

        if (health != null && controller != null)
        {
            // Якщо Айвен невразливий
            if (controller.isInvincible) return;

            // 1. Наносимо урон
            health.TakeDamage(spikeDamage);
            controller.OnTakeDamage();

            // 2. Фізика відкидання
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;

                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;

                knockbackDirection.y = 1f;

                rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}