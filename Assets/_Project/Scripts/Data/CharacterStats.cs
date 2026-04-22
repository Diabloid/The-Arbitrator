using UnityEngine;
using System;

public abstract class CharacterStats : ScriptableObject
{
    [Header("Базові параметри")]
    public int maxHealth = 100;
    public float moveSpeed = 5f;

    [Header("Базовий Бій")]
    public int baseDamage = 10;
    public float attackRate = 2f;
}