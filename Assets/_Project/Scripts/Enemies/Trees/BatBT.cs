using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

public class BatBT : BTree
{
    [Header("Налаштування Миші")]
    public EnemyStats enemyStats;
    public LayerMask playerLayer;
    public Vector2 visionBox = new Vector2(15f, 15f);
    public GameObject alertPrefab;

    protected override Node SetupTree()
    {
        Animator animator = GetComponent<Animator>();
        EnemyCombat combat = GetComponent<EnemyCombat>();

        // Вибираємо сторону: 50% шанс бути зліва, 50% шанс бути справа
        float sideMultiplier = Random.value > 0.5f ? 1f : -1f;

        // Вибираємо дистанцію (від 0.8 до 1.5 метра вбік) та висоту (від 1 до 2 метрів вгору)
        float flankX = Random.Range(0.8f, 1.5f) * sideMultiplier;
        float flankY = Random.Range(0.8f, 1.5f);

        Vector2 myFlankPosition = new Vector2(flankX, flankY);

        // 1. АТАКА 
        Node attackSeq = new Sequence(new List<Node>
        {
            new CheckInStrikePosition(transform, myFlankPosition, 0.8f),
            new CheckAttackBox(transform, enemyStats.attackBox, enemyStats.attackOffset, playerLayer),
            new TaskAttack(transform, enemyStats, animator, enemyStats.attackRate, 2)
        });

        // 2. ПОЛЬОТНА ПОГОНЯ
        Node chaseSeq = new Sequence(new List<Node>
        {
            new CheckTargetInFOV(transform, visionBox, Vector2.zero, playerLayer, alertPrefab, default),
            new TaskFlyChase(transform, enemyStats.moveSpeed, animator, myFlankPosition)
        });

        // 3. ЗБИРАЄМО ДЕРЕВО
        Node root = new Sequence(new List<Node>
        {
            new CheckAwakeState(combat),
            new Selector(new List<Node>
            {
                new CheckAttackState(combat),
                attackSeq,
                chaseSeq
            })
        });

        return root;
    }

    // Допоміжний метод для налаштування зору в Інспекторі
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, visionBox);
    }
}