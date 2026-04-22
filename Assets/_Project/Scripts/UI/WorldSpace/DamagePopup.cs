using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;  // Посилання на текст всередині префаба

    private Vector3 _moveVector;    // Вектор руху для дуги
    private float _disappearTimer;  // Таймер для зникнення
    private Color _textColor;       // Початковий колір тексту

    private const float disappearTimer_max = 1f; // Скільки живе текст (1 сек)

    private void Awake()
    {
        _textMesh = transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        // 1. Встановлюємо текст
        if (_textMesh != null)
        {
            _textMesh.text = damageAmount.ToString();
            _textColor = _textMesh.color;

            if (isCritical)
            {
                transform.localScale *= 1.5f;

                Color orangeColor = new Color(1f, 0.5f, 0f);

                _textMesh.color = orangeColor;
                _textColor = orangeColor;
            }
        }

        // 2. Рандомний рух по дузі
        _moveVector = new Vector3(Random.Range(-1f, 1f) * 1.5f, 3f, 0);

        _disappearTimer = disappearTimer_max;
    }

    private void Update()
    {
        transform.position += _moveVector * Time.deltaTime;
        _moveVector -= _moveVector * 3f * Time.deltaTime;
        _moveVector.y -= 5f * Time.deltaTime;

        _disappearTimer -= Time.deltaTime;

        if (_disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            _textColor.a -= fadeSpeed * Time.deltaTime;

            if (_textMesh != null)
                _textMesh.color = _textColor;

            if (_textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}