using UnityEngine;
using System.Collections;

public class ArenaController : MonoBehaviour
{
    [Header("Піки")]
    public Transform leftSpikes;
    public Transform rightSpikes;

    [Header("Налаштування")]
    public float undergroundOffset = 3f;
    public float popupSpeed = 15f;
    public float fadeSpeed = 5f;

    [Header("Звук")]
    [SerializeField] private AudioClip _spikeMoveSound;

    private Vector3 _leftClosed, _leftOpen;
    private Vector3 _rightClosed, _rightOpen;

    private SpriteRenderer _leftRenderer;
    private SpriteRenderer _rightRenderer;

    private void Start()
    {
        if (leftSpikes != null)
        {
            _leftOpen = leftSpikes.position;
            _leftClosed = _leftOpen - new Vector3(0, undergroundOffset, 0);
            leftSpikes.position = _leftClosed;

            _leftRenderer = leftSpikes.GetComponent<SpriteRenderer>();
            SetAlpha(_leftRenderer, 0f);
        }

        if (rightSpikes != null)
        {
            _rightOpen = rightSpikes.position;
            _rightClosed = _rightOpen - new Vector3(0, undergroundOffset, 0);
            rightSpikes.position = _rightClosed;

            _rightRenderer = rightSpikes.GetComponent<SpriteRenderer>();
            SetAlpha(_rightRenderer, 0f);
        }
    }

    public void LockArena()
    {
        StopAllCoroutines();
        if (leftSpikes) StartCoroutine(MoveAndFadeSpikes(leftSpikes, _leftRenderer, _leftOpen, 1f));
        if (rightSpikes) StartCoroutine(MoveAndFadeSpikes(rightSpikes, _rightRenderer, _rightOpen, 1f));

        if (AudioManager.Instance != null && _spikeMoveSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_spikeMoveSound, 0.5f, 0.95f, 1.1f);
        }

        Debug.Log("<color=red>АРЕНА ЗАЧИНЕНА!</color>");
    }

    public void UnlockArena()
    {
        StopAllCoroutines();
        if (leftSpikes) StartCoroutine(MoveAndFadeSpikes(leftSpikes, _leftRenderer, _leftClosed, 0f));
        if (rightSpikes) StartCoroutine(MoveAndFadeSpikes(rightSpikes, _rightRenderer, _rightClosed, 0f));

        if (AudioManager.Instance != null && _spikeMoveSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_spikeMoveSound, 0.4f, 0.75f, 0.9f);
        }

        Debug.Log("<color=green>АРЕНА ВІДЧИНЕНА!</color>");
    }

    private IEnumerator MoveAndFadeSpikes(Transform spikes, SpriteRenderer sr, Vector3 targetPos, float targetAlpha)
    {
        bool isMoving = true;
        bool isFading = true;

        while (isMoving || isFading)
        {
            // 1. Рух
            if (Vector3.Distance(spikes.position, targetPos) > 0.01f)
            {
                spikes.position = Vector3.MoveTowards(spikes.position, targetPos, popupSpeed * Time.deltaTime);
            }
            else
            {
                spikes.position = targetPos;
                isMoving = false;
            }

            // 2. Альфа
            isFading = false;
            if (sr != null)
            {
                if (Mathf.Abs(sr.color.a - targetAlpha) > 0.01f)
                {
                    Color c = sr.color;
                    c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
                    sr.color = c;
                    isFading = true;
                }
                else
                {
                    Color c = sr.color;
                    c.a = targetAlpha;
                    sr.color = c;
                }
            }

            yield return null;
        }
    }

    // Допоміжний метод
    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}