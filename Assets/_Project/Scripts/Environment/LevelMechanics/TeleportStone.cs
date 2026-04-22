using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportStone : MonoBehaviour, IInteractable
{
    [Header("Режим Каменя")]
    [Tooltip("True = Камінь Відправляє. False = Камінь у Хабі (приймає).")]
    [SerializeField] private bool isInteractive = true;
    [SerializeField] private string destinationScene = "Hub";

    [Header("Візуал")]
    [SerializeField] private GameObject activationVisual;
    [SerializeField] private float teleportDelay = 1.5f;

    [Header("Інтеракт (Тільки для відправника)")]
    [SerializeField] private string promptText = "Повернутися в Хаб";
    [SerializeField] private float promptHeight = 1.5f;

    // Властивості інтерфейсу IInteractable
    public string InteractionPrompt => promptText;
    public float PromptHeight => promptHeight;

    private bool _isTeleporting = false;

    private void Start()
    {
        if (activationVisual != null) activationVisual.SetActive(false);

        if (!isInteractive && PlayerPrefs.GetInt("IsTeleportingToHub", 0) == 1)
        {
            PlayerPrefs.SetInt("IsTeleportingToHub", 0);
            StartCoroutine(ReceivePlayerRoutine());
        }
    }

    // ВІДПРАВКА
    public void Interact()
    {
        // Якщо це камінь-приймач або ми вже в процесі - ігноруємо
        if (!isInteractive || _isTeleporting) return;

        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        _isTeleporting = true;

        // 1. Блокуємо рух гравця
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.canMove = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 2. Плавне вмикання світіння
        if (activationVisual != null)
        {
            activationVisual.SetActive(true);
            SpriteRenderer sr = activationVisual.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;

                float elapsed = 0f;
                while (elapsed < teleportDelay)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Lerp(0f, 1f, elapsed / teleportDelay);
                    sr.color = c;
                    yield return null;
                }
                c.a = 1f;
                sr.color = c;
            }
        }
        else
        {
            yield return new WaitForSeconds(teleportDelay);
        }

        PlayerPrefs.SetInt("IsTeleportingToHub", 1);
        PlayerPrefs.Save();

        LoadingScreenManager.Instance.LoadScene(destinationScene);
    }

    // ПРИЙОМ
    private IEnumerator ReceivePlayerRoutine()
    {
        yield return null;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            // Ставимо Айвена біля каменя
            player.transform.position = transform.position + new Vector3(0, -0.5f, 0);

            if (activationVisual != null)
            {
                activationVisual.SetActive(true);
                SpriteRenderer sr = activationVisual.GetComponent<SpriteRenderer>();

                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f;
                    sr.color = c;

                    yield return new WaitForSeconds(0.5f);

                    float elapsed = 0f;
                    float fadeOutTime = 1.5f;
                    while (elapsed < fadeOutTime)
                    {
                        elapsed += Time.deltaTime;
                        c.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                        sr.color = c;
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(2f);
                }

                activationVisual.SetActive(false);
            }
        }
    }
}