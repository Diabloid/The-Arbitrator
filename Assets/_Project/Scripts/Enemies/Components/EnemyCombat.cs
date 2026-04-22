using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Стати")]
    public EnemyStats enemyStats;
    public LayerMask playerLayer;

    [Header("Блок (Для Танків)")]
    public bool canBlock = false;
    public float blockDuration = 2f;
    public bool hasSuperArmor = false;

    [Header("Ухилення (Для асасіна)")]
    public bool jumpBackAfterAttack = false;
    public float evadeDuration = 0.3f;

    [Header("Стан")]
    public bool isAttacking = false;
    public bool isBlocking = false;
    public bool isEvading = false;
    public bool readyToTeleport = false;

    [Header("Летючі вороги")]
    public bool isAwake = true;

    public void DealDamageEvent()
    {
        if (enemyStats == null) return;

        float facingDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 centerPoint = (Vector2)transform.position + new Vector2(enemyStats.attackOffset.x * facingDir, enemyStats.attackOffset.y);

        Collider2D player = Physics2D.OverlapBox(centerPoint, enemyStats.attackBox, 0, playerLayer);

        if (player != null)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(enemyStats.baseDamage, false);
                Debug.Log($"<color=red>УДАР ПО ГРАВЦЮ!</color> Нанесено {enemyStats.baseDamage} урону.");
            }
        }
        else
        {
            Debug.Log("<color=yellow>Промах!</color> Гравця немає в зоні удару під час анімації.");
        }
    }

    public void StartAttackWithFailsafe()
    {
        isAttacking = true;
        CancelInvoke(nameof(AnimationEvent_EndAttack));
        // ЗАПОБІЖНИК: Якщо через 1.5 секунди анімація не вимкне прапорець, скрипт зробить це сам
        Invoke(nameof(AnimationEvent_EndAttack), 1.5f);
    }

    public void AnimationEvent_EndAttack()
    {
        isAttacking = false;
        CancelInvoke(nameof(AnimationEvent_EndAttack));
        if (canBlock)
        {
            StartBlock();
        }

        if (jumpBackAfterAttack)
        {
            isEvading = true;
            Invoke(nameof(EndEvade), evadeDuration);
        }
    }

    public void AnimationEvent_WakeUpComplete()
    {
        isAwake = true;
        Debug.Log($"{gameObject.name} повністю прокинулась і готова атакувати!");
    }

    public void EndEvade()
    {
        isEvading = false;
        readyToTeleport = true;
        CancelInvoke(nameof(EndEvade));
    }

    public void StartBlock()
    {
        isBlocking = true;
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("isBlocking", true);

        // Автоматично опускаємо щит через заданий час
        Invoke(nameof(EndBlock), blockDuration);
    }

    public void EndBlock()
    {
        isBlocking = false;
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("isBlocking", false);
        CancelInvoke(nameof(EndBlock));
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyStats == null) return;

        float facingDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 centerPoint = (Vector2)transform.position + new Vector2(enemyStats.attackOffset.x * facingDir, enemyStats.attackOffset.y);

        // Пурпуровий колір для бойового хітбоксу
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawWireCube(centerPoint, enemyStats.attackBox);
    }
}