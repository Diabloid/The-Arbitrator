using UnityEngine;

public class InteractableNote : MonoBehaviour, IInteractable
{
    [Header("Налаштування записки")]
    public DialogueNode noteContent;

    [Header("Налаштування UI")]
    [SerializeField] private string promptText = "Прочитати записку";
    [SerializeField] private float promptHeightOffset = 1.0f;

    [Header("Звук")]
    [SerializeField] private AudioClip _noteReadSound;

    public string InteractionPrompt => promptText;
    public float PromptHeight => promptHeightOffset;

    // Реалізація головного методу
    public void Interact()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive)
        {
            if (AudioManager.Instance != null && _noteReadSound != null)
            {
                AudioManager.Instance.PlaySFXRandomPitch(_noteReadSound, 1f, 0.9f, 1.1f);
            }

            DialogueManager.Instance.StartDialogue(noteContent, transform);
        }
    }
}