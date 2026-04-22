using UnityEngine;

public class HubStateManager : MonoBehaviour
{
    [Header("Дані гравця")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Об'єкти оточення")]
    [SerializeField] private GameObject wantedPoster;
    [SerializeField] private GameObject tornFlag;
    [SerializeField] private GameObject repairedFlag;

    [Header("NPC та Діалоги")]
    [SerializeField] private NPCInteract[] hubNPCs;
    [SerializeField] private DialogueNode chaosDialogueNode;
    [SerializeField] private DialogueNode orderDialogueNode;

    private void Start()
    {
        if (playerStats == null)
        {
            return;
        }

        CheckHubState();
    }

    private void CheckHubState()
    {
        // Якщо гравець ще не має очок моралі — Хаб залишається у стандартному вигляді
        if (playerStats.chaosPoints == 0 && playerStats.lawPoints == 0)
        {
            Debug.Log("[HubStateManager] Хаб у стандартному стані (виборів ще не було).");
            return;
        }

        // Перевіряємо Хаос (Помилування)
        if (playerStats.chaosPoints > 0)
        {
            SetupChaosState();
        }
        // Перевіряємо Порядок (Вбивство)
        else if (playerStats.lawPoints > 0)
        {
            SetupLawState();
        }
    }

    private void SetupChaosState()
    {
        Debug.Log("<color=magenta>[ХАБ] Завантажено стан: ХАОС</color>");

        if (wantedPoster != null) wantedPoster.SetActive(false);

        if (repairedFlag != null) repairedFlag.SetActive(false);
        if (tornFlag != null) tornFlag.SetActive(true);

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearAllMissions();
        }

        UpdateNPCDialogues(chaosDialogueNode);
    }

    private void SetupLawState()
    {
        Debug.Log("<color=blue>[ХАБ] Завантажено стан: ПОРЯДОК</color>");

        if (wantedPoster != null) wantedPoster.SetActive(false);

        if (tornFlag != null) tornFlag.SetActive(false);
        if (repairedFlag != null) repairedFlag.SetActive(true);

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearAllMissions();
        }

        UpdateNPCDialogues(orderDialogueNode);
    }

    private void UpdateNPCDialogues(DialogueNode newNode)
    {
        if (newNode == null)
        {
            return;
        }

        foreach (var npc in hubNPCs)
        {
            if (npc != null)
            {
                npc.SetDialogueNode(newNode);
            }
        }
    }
}