using UnityEngine;
using System.Collections;

public class HitShake : MonoBehaviour
{
    [Header("Налаштування тряски")]
    [SerializeField] private float _shakeDuration = 0.15f; // Як довго трясеться (в секундах)
    [SerializeField] private float _shakeMagnitude = 0.1f; // Сила тряски (на скільки пікселів відхиляється)

    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _originalPos = transform.localPosition;
    }

    public void TriggerShake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }
        _shakeCoroutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            float randomX = Random.Range(-1f, 1f) * _shakeMagnitude;

            transform.localPosition = new Vector3(_originalPos.x + randomX, _originalPos.y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
    }
}