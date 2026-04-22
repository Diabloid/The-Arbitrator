using UnityEngine;

public class BoardInteract : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Дошка справ";
    [SerializeField] private float promptHeight = 0.5f;
    public float PromptHeight => promptHeight;

    [Header("Тимчасово для тесту")]
    public MissionData testMission;

    public void Interact()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

        Debug.Log("Відкривається інтерфейс Дошки справ...");

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OpenBoard();
        }
    }
}