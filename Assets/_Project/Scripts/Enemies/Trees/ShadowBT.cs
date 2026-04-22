using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShadowBT : BTree
{
    [Header("Стати Ассасина")]
    public EnemyStats enemyStats;

    [Header("Зір")]
    public Vector2 visionBox = new Vector2(12f, 3f);
    public Vector2 visionOffset = new Vector2(4f, 0f);
    public GameObject alertPrefab;

    [Header("Оточення")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask playerLayer;
    public UnityEvent onAggro;

    protected override Node SetupTree()
    {
        float moveSpeed = (enemyStats != null) ? enemyStats.moveSpeed : 6f;
        float cooldown = (enemyStats != null) ? enemyStats.attackRate : 0.8f;
        Vector2 box = (enemyStats != null) ? enemyStats.attackBox : new Vector2(1.5f, 1f);
        Vector2 offset = (enemyStats != null) ? enemyStats.attackOffset : new Vector2(0.5f, 0f);

        Animator animator = GetComponent<Animator>();
        EnemyCombat combat = GetComponent<EnemyCombat>();

        CloakController cloak = GetComponent<CloakController>();
        if (cloak == null) cloak = gameObject.AddComponent<CloakController>();

        // 1. АТАКА (Найвищий пріоритет)
        Node attackSeq = new Sequence(new List<Node>
        {
            new CheckAttackBox(transform, box, offset, playerLayer),
            new TaskSetCloak(cloak, false),
            new TaskAttack(transform, enemyStats, animator, cooldown, 1, 0f)
        });

        // 2. ПЕРЕСЛІДУВАННЯ (Середній пріоритет)
        Node chaseSeq = new Sequence(new List<Node>
        {
            new CheckTargetInFOV(transform, visionBox, visionOffset, playerLayer, alertPrefab, groundLayer, () => onAggro?.Invoke()),
            new TaskSetCloak(cloak, true),
            new TaskChase(transform, moveSpeed, animator, groundLayer, wallLayer)
        });

        // 3. ЗАСІДКА (Найнижчий пріоритет)
        Node ambushSeq = new Sequence(new List<Node>
        {
            new TaskSetCloak(cloak, false),
            new TaskStandStill(transform, animator)
        });

        // 4. ЗБИРАЄМО ДЕРЕВО
        Node root = new Selector(new List<Node>
        {
            new TaskEvade(transform, combat, animator, cloak, 12f),
            new TaskFlankTeleport(transform, combat, groundLayer, 5f, 0.5f),
            new CheckAttackState(combat),
            attackSeq,
            chaseSeq,
            ambushSeq
        });

        return root;
    }

    // Гізмо для візуалізації
    private void OnDrawGizmos()
    {
        if (transform == null) return;

        UnityEngine.Transform groundCheck = transform.Find("GroundCheck");
        float facingDir = transform.localScale.x > 0 ? 1 : -1;

        // Жовта зона (Зір)
        Gizmos.color = Color.yellow;
        Vector2 visionCenter = (Vector2)transform.position + new Vector2(visionOffset.x * facingDir, visionOffset.y);
        Gizmos.DrawWireCube(visionCenter, visionBox);

        // Червона зона (Атака)
        if (enemyStats != null)
        {
            Gizmos.color = Color.red;
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(enemyStats.attackOffset.x * facingDir, enemyStats.attackOffset.y);
            Gizmos.DrawWireCube(attackCenter, enemyStats.attackBox);
        }

        if (groundCheck != null)
        {
            // Земля (Білий промінь вниз)
            Gizmos.color = Color.white;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 1f);

            // Стіна (Жовтий промінь вперед)
            Gizmos.color = Color.yellow;
            Vector3 direction = transform.right * facingDir;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + direction * 0.5f);
        }
    }
}