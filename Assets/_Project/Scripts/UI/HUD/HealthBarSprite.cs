using UnityEngine;
using UnityEngine.UI;

public class HealthBarSprite : MonoBehaviour
{
    [Header("Залежність")]
    public EntityHealth playerHealth;

    [Header("Компоненти")]
    public Image barImage;      // Image, який буде змінюватися
    public Sprite[] barSprites; // Масив спрайтів для різних рівнів здоров'я (від 0% до 100%)

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateVisuals;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateVisuals;
    }

    private void UpdateVisuals(int current, int max)
    {
        if (max == 0 || barImage == null || barSprites.Length == 0) return;

        float percent = (float)current / max;
        int index = Mathf.RoundToInt(percent * (barSprites.Length - 1));

        index = Mathf.Clamp(index, 0, barSprites.Length - 1);

        barImage.sprite = barSprites[index];
    }
}