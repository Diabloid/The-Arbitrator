using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Поточна місія")]
    public MissionData currentMission;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AcceptMission(MissionData mission)
    {
        currentMission = mission;
    }

    public void ClearMission()
    {
        currentMission = null;
    }
}