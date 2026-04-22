using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    [Header("Фонова музика сцени")]
    [SerializeField] private AudioClip _sceneMusic;

    private void Start()
    {
        Invoke(nameof(PlayMusic), 0.1f);
    }

    private void PlayMusic()
    {
        if (AudioManager.Instance != null && _sceneMusic != null)
        {
            AudioManager.Instance.PlayMusic(_sceneMusic);
        }
    }
}