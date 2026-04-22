using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Stats/Enemy Data")]
public class EnemyStats : CharacterStats
{
    [Header("Зона удару (Hitbox)")]
    public Vector2 attackBox = new Vector2(1.5f, 1f);
    public Vector2 attackOffset = new Vector2(0.5f, 0f);

    [Header("Налаштування Луту")]
    public int minCoins = 1;
    public int maxCoins = 3;
    public float healGemDropChance = 20f;
}