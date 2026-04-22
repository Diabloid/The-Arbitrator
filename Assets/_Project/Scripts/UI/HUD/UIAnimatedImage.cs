using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIAnimatedImage : MonoBehaviour
{
    [Header("Налаштування анімації")]
    [SerializeField] private Sprite[] animationSprites; // Масив кадрів
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private bool ignoreTimeScale = true;

    [Header("Колір")]
    [SerializeField] private Color startColor = Color.white;

    [Header("Пауза")]
    [SerializeField] private float pauseDuration = 3f;  // Скільки секунд чекати після одного оберту
    [SerializeField] private bool randomPause = false;  // Чи робити паузу випадковою?

    private Image _targetImage;

    void Awake()
    {
        _targetImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        if (_targetImage != null)
        {
            _targetImage.color = startColor;
        }

        if (animationSprites == null || animationSprites.Length == 0)
        {
            Debug.LogWarning($"У об'єкта {name} немає спрайтів для анімації UI!");
            return;
        }

        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float frameWait = 1f / frameRate;

        WaitForSeconds scaledFrameWait = new WaitForSeconds(frameWait);

        while (true)
        {
            // 1. Програємо повний цикл анімації
            for (int i = 0; i < animationSprites.Length; i++)
            {
                _targetImage.sprite = animationSprites[i];

                if (ignoreTimeScale)
                    yield return new WaitForSecondsRealtime(frameWait);
                else
                    yield return scaledFrameWait;
            }

            // 2. Встановлюємо перший кадр
            _targetImage.sprite = animationSprites[0];

            // 3. Чекаємо паузу між циклами
            float wait = randomPause ? Random.Range(pauseDuration * 0.5f, pauseDuration * 1.5f) : pauseDuration;

            if (ignoreTimeScale)
                yield return new WaitForSecondsRealtime(wait);
            else
                yield return new WaitForSeconds(wait);
        }
    }
}