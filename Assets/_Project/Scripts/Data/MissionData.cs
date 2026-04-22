using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Arbitrator/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Базова інформація")]
    public string missionID; // Унікальний ідентифікатор
    public string missionTitle; // Назва для UI

    [TextArea(3, 5)]
    public string missionDescription; // Офіційний текст вироку від Імперії

    [Header("Ціль")]
    public string targetName; // Хто наша ціль
    public string locationName; // Назва локації
    public string sceneToLoad; // Точна назва сцени в Unity, яку треба завантажити при старті місії

    [Header("Нагороди (Базові)")]
    public int baseGoldReward;

    [Header("Нотатки")]
    [TextArea(2, 4)]
    public string arbitratorNotes; // Можливі думки Вел щодо цієї справи
}