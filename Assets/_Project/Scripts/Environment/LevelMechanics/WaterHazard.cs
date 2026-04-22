using UnityEngine;
using System.Collections;

public class WaterHazard : MonoBehaviour
{
    [Header("Налаштування")]
    public int waterDamage = 20;
    public float teleportDelay = 0.3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(HandleWaterFall(collision.gameObject));
        }
    }

    private IEnumerator HandleWaterFall(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        EntityHealth health = player.GetComponent<EntityHealth>();

        if (controller != null)
        {
            controller.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        bool isFatal = false;
        if (health != null)
        {
            // Якщо поточне ХП менше або дорівнює урону від води — це смерть
            if (health.currentHealth <= waterDamage)
            {
                isFatal = true;
            }

            health.TakeDamage(waterDamage);
        }

        // Якщо удар смертельний — скасовуємо телепортацію і виходимо з корутини
        if (isFatal)
        {
            Debug.Log("Вода забрала останні сили Айвена...");
            yield break;
        }

        // Якщо гравець вижив - даємо йому I-Frames, чекаємо і телепортуємо
        if (controller != null)
        {
            controller.OnTakeDamage();
        }

        yield return new WaitForSeconds(teleportDelay);

        if (controller != null)
        {
            player.transform.position = controller.lastSafePosition;
            Debug.Log("Айвен вибрався з токсичної води!");
        }
    }
}