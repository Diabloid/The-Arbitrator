using UnityEngine;

public class NPCInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueNode startDialogueNode;
    [SerializeField] private string npcName = "Мандрівник";
    [SerializeField] private float promptHeight = 0.5f;

    public string InteractionPrompt => $"Поговорити з {npcName}";
    public float PromptHeight => promptHeight;

    public void Interact()
    {
        // Звертаємося до Менеджера і передаємо йому файл
        if (DialogueManager.Instance != null && startDialogueNode != null)
        {
            DialogueManager.Instance.StartDialogue(startDialogueNode, transform);
        }
        else
        {
            Debug.LogError("Не знайдено DialogueManager або файл діалогу!");
        }
    }

    public void SetDialogueNode(DialogueNode newNode)
    {
        startDialogueNode = newNode;
    }
}