using UnityEngine;

public class AlertEffect : MonoBehaviour
{
    public float moveSpeed = 1f;    // Швидкість руху вгору
    public float lifeTime = 1.5f;   // Час життя ефекту
    private SpriteRenderer _sr;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Рух вгору
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // Плавне зникнення
        if (_sr != null)
        {
            Color color = _sr.color;
            color.a -= (1f / lifeTime) * Time.deltaTime;
            _sr.color = color;
        }
    }
}