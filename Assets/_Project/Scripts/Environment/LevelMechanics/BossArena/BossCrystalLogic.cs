using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossCrystalLogic : MonoBehaviour
{
    [Header("Левітація")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    [Header("Смерть")]
    public GameObject shatterVFXPrefab;

    private Vector3 _startPos;
    private EntityHealth _health;
    private bool _isDead = false;

    private void Start()
    {
        _startPos = transform.position;
        _health = GetComponent<EntityHealth>();

        if (_health != null) _health.OnDeath += HandleDeath;
    }

    private void Update()
    {
        if (_isDead) return;

        // Левітація
        float newY = _startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(_startPos.x, newY, _startPos.z);
    }

    private void HandleDeath()
    {
        _isDead = true;

        if (shatterVFXPrefab != null) Instantiate(shatterVFXPrefab, transform.position, Quaternion.identity);

        Light2D light = GetComponentInChildren<Light2D>();
        if (light != null) light.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (_health != null) _health.OnDeath -= HandleDeath;
    }
}