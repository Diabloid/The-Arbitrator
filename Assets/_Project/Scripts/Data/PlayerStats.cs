using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Stats/Player Data")]
public class PlayerStats : CharacterStats
{
    [Header("Економіка")]
    public int currentCurrency = 0;

    [Header("Мораль (Лор)")]
    public int lawPoints = 0;
    public int chaosPoints = 0;

    [Header("Інвентар")]
    public int currentHealGems = 2;
    public int maxHealGems = 3;
    public int healAmount = 20;

    [Header("Звуки UI / Інвентарю")]
    [SerializeField] private AudioClip _inventoryFullSound;

    [Header("Рух")]
    public float jumpForce = 16f;

    [Header("Критичний удар")]
    public float critChance = 20f;
    public float critMultiplier = 2f;

    [Header("Атака з повітря (Plunge)")]
    public int airAttackDamage = 40;
    public float airAttackCooldown = 3f;
    public float plungeSpeed = 25f;
    public float plungeRadius = 1.5f;
    public float impactRecoveryTime = 0.5f;

    public event Action OnCurrencyChanged;
    public event Action OnHealGemsChanged;
    public event Action OnHealGemFull;
    public event Action OnMoralityChanged;

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        if (currentCurrency < 0) currentCurrency = 0;
        OnCurrencyChanged?.Invoke();
        Debug.Log($"Гаманець: {currentCurrency} (+{amount})");
    }

    public void ResetCurrency()
    {
        currentCurrency = 0;
        OnCurrencyChanged?.Invoke();
    }

    public bool AddHealGem(int amount)
    {
        if (currentHealGems >= maxHealGems)
        {
            Debug.Log("Інвентар повний!");
            return false;
        }

        currentHealGems = Mathf.Min(currentHealGems + amount, maxHealGems);
        OnHealGemsChanged?.Invoke();
        Debug.Log($"Камнів Зцілення: {currentHealGems}/{maxHealGems}");
        return true;
    }

    public void TriggerHealGemFull()
    {
        OnHealGemFull?.Invoke();

        if (AudioManager.Instance != null && _inventoryFullSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_inventoryFullSound, 0.8f, 0.95f, 1.05f);
        }
    }

    public bool UseHealGem()
    {
        if (currentHealGems > 0)
        {
            currentHealGems--;
            OnHealGemsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddLawPoints(int amount)
    {
        lawPoints += amount;
        OnMoralityChanged?.Invoke();
        Debug.Log($"<color=blue>[ЛОЯЛЬНІСТЬ]</color> Отримано Порядок: +{amount}. Загалом: {lawPoints}");
    }

    public void AddChaosPoints(int amount)
    {
        chaosPoints += amount;
        OnMoralityChanged?.Invoke();
        Debug.Log($"<color=magenta>[ЗРАДА]</color> Отримано Хаос: +{amount}. Загалом: {chaosPoints}");
    }
}