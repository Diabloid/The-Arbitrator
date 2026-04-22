using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    // 1. Статистика та ресурси
    public int currency;    // Кількість монет
    public int healGems;    // Кількість камнів
    public int baseDamage;  // Базовий урон
    public int lawPoints;   // Показник Закону
    public int chaosPoints; // Показник Хаосу
    public int currentHealth; // Поточне здоров'я

    // 2. Прогрес (Місія)
    public string activeMissionID; // Зберігаємо тільки ID

    // 3. Позиція у світі
    public string currentScene; // Назва сцени, в якій знаходиться гравець
    public float posX; // Позиція по X
    public float posY; // Позиція по Y
    public float posZ; // Позиція по Z

    // 4. Список ID вбитих ворогів
    public List<string> deadEnemies = new List<string>();
}