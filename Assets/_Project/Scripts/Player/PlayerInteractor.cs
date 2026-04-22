using UnityEngine;
using TMPro; // 🔥 Для роботи з текстом

public class PlayerInteractor : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private float interactionRadius = 1.5f; // Радіус пошуку предметів для взаємодії
    [SerializeField] private LayerMask interactionLayer; // Шар, на якому лежать предмети для взаємодії
    [SerializeField] private Transform interactionPoint; // Центр гравця

    [Header("UI")]
    [SerializeField] private GameObject promptPrefab; // Префаб "[E] Name"

    private GameObject _currentPromptObj;
    private TextMeshProUGUI _promptText;
    private IInteractable _currentTarget;

    void Start()
    {
        if (promptPrefab != null)
        {
            _currentPromptObj = Instantiate(promptPrefab);
            _promptText = _currentPromptObj.GetComponentInChildren<TextMeshProUGUI>();
            _currentPromptObj.SetActive(false);
        }
    }

    void Update()
    {
        ScanForInteractables();

        // Натискання Е
        if (Input.GetKeyDown(KeyCode.E) && _currentTarget != null)
        {
            _currentTarget.Interact();

            if (_currentPromptObj != null) _currentPromptObj.SetActive(false);
            _currentTarget = null;
        }
    }

    private void ScanForInteractables()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(interactionPoint.position, interactionRadius, interactionLayer);

        IInteractable bestTarget = null;
        float closestDistance = float.MaxValue;
        Collider2D bestCollider = null;

        foreach (Collider2D col in hitObjects)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null) continue;
            Vector2 directionToTarget = col.transform.position - interactionPoint.position;
            float facingDir = transform.localScale.x;

            // Фільтр "Спереду"
            if (directionToTarget.x * facingDir > 0)
            {
                // Фільтр "Найближче"
                float dist = Vector2.Distance(interactionPoint.position, col.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = interactable;
                    bestCollider = col;
                }
            }
        }

        // Зчитуємо стани наших UI - менеджерів
        bool isDialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;
        bool isBoardActive = BoardManager.Instance != null && BoardManager.Instance.IsBoardOpen;

        if (isDialogueActive || isBoardActive)
        {
            if (_currentPromptObj.activeSelf)
            {
                _currentPromptObj.SetActive(false);
            }

            _currentTarget = null;

            return;
        }

        // Оновлюємо UI
        if (bestTarget != _currentTarget)
        {
            _currentTarget = bestTarget;

            if (_currentTarget != null)
            {
                _currentPromptObj.SetActive(true);

                if (_promptText != null)
                {
                    _promptText.text = $"[E] {_currentTarget.InteractionPrompt}";
                }

                _currentPromptObj.transform.position = bestCollider.transform.position + Vector3.up * _currentTarget.PromptHeight;
            }
            else
            {
                _currentPromptObj.SetActive(false);
            }
        }
        else if (_currentTarget != null && bestCollider != null)
        {
            _currentPromptObj.transform.position = bestCollider.transform.position + Vector3.up * _currentTarget.PromptHeight;
        }
    }
}