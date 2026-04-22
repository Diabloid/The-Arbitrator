using UnityEngine;

// 1. Структура для відповіді гравця
[System.Serializable]
public struct DialogueChoice
{
    [TextArea(1, 2)]
    public string choiceText; // Що каже гравець

    public DialogueNode nextNode; // До якого файлу діалогу веде цей вибір

    [Header("Подія (необов'язково)")]
    public string actionId; // Кодове слово для тригера
}

// 2. Сам файл діалогу (Вузол)
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue System/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Репліка NPC")]
    public string speakerName = "Мандрівник";

    public Color speakerColor = Color.white;
    public Color speakerOutlineColor = Color.black;
    [Range(0f, 1f)] public float speakerOutlineWidth = 0.2f;

    [Header("Озвучка персонажа (Опціонально)")]
    public AudioClip voiceSound;
    [Range(0.5f, 2f)] public float voicePitch = 1f;

    [TextArea(3, 5)]
    public string npcText;

    [Header("Варіанти відповіді (якщо пусто - діалог закінчиться)")]
    public DialogueChoice[] choices;
}