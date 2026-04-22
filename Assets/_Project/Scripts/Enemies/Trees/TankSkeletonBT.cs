using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

public class TankSkeletonBT : BTree
{
    [Header("Стати")]
    public EnemyStats enemyStats;

    [Header("Зір (Vision)")]
    public Vector2 visionBox = new Vector2(10f, 3f);
    public Vector2 visionOffset = new Vector2(2f, 0f);
    public GameObject alertPrefab;

    [Header("Оточення")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask playerLayer;

    [Header("Поведінка")]
    public bool startFacingRight = true; // Почати, дивлячись вправо чи вліво?
    public float patrolRange = -1f;      // -1 = без ліміту, 5 = ходити 5 метрів від старту
    public float attackCooldown = 2.0f;

    protected override Node SetupTree()
    {
        float moveSpeed = (enemyStats != null) ? enemyStats.moveSpeed : 2f;
        float cooldown = (enemyStats != null) ? enemyStats.attackRate : 2f;

        Vector2 box = (enemyStats != null) ? enemyStats.attackBox : new Vector2(1.5f, 1f);
        Vector2 offset = (enemyStats != null) ? enemyStats.attackOffset : new Vector2(0.5f, 0f);

        Animator animator = GetComponent<Animator>();
        EnemyCombat combat = GetComponent<EnemyCombat>();

        // 1. АТАКА ТА БЛОК (Вискорий пріоритет)
        Node combatSeq = new Sequence(new List<Node>
        {
            new CheckAttackBox(transform, enemyStats.attackBox, enemyStats.attackOffset, playerLayer),
            new TaskAttack(transform, enemyStats, animator, enemyStats.attackRate, 3)
        });

        // 2. ПЕРЕСЛІДУВАННЯ (Середній пріоритет)
        Node chaseSeq = new Sequence(new List<Node>
        {
            new CheckTargetInFOV(transform, visionBox, visionOffset, playerLayer, alertPrefab, groundLayer),
            new TaskChase(transform, moveSpeed * 1.5f, animator, groundLayer, wallLayer)
        });

        // 3. ПАТРУЛЬ (Найнижчий пріоритет)
        Node patrolTask = new TaskPatrol(
            transform, moveSpeed, groundLayer, wallLayer, startFacingRight, patrolRange, animator
        );

        Node root = new Selector(new List<Node>
        {
            new CheckAttackState(combat),
            new CheckBlockState(combat, GetComponent<Rigidbody2D>()),
            combatSeq,
            chaseSeq,
            patrolTask
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
        Gizmos.color = Color.red;
        Vector2 attackCenter = (Vector2)transform.position + new Vector2(enemyStats.attackOffset.x * facingDir, enemyStats.attackOffset.y);
        Gizmos.DrawWireCube(attackCenter, enemyStats.attackBox);

        if (groundCheck != null)
        {
            // 2. Земля (Червоний вниз)
            Gizmos.color = Color.white;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 1f);

            // 3. Стіна (Жовтий вперед)
            Gizmos.color = Color.yellow;
            Vector3 direction = transform.right * (transform.localScale.x > 0 ? 1 : -1);
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + direction * 0.5f);
        }

        // 4. Зона патрулювання (Синя сфера)
        if (patrolRange > 0)
        {
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawWireSphere(Application.isPlaying ? transform.position : transform.position, patrolRange);
        }
    }
}