public interface IInteractable
{
    void Interact();
    string InteractionPrompt { get; }
    float PromptHeight { get;  }
}