using UnityEngine;

public class DripFX : MonoBehaviour
{
    [Header("Звук краплі")]
    [SerializeField] private AudioClip _dripSound;

    private void OnParticleCollision(GameObject other)
    {
        if (AudioManager.Instance != null && _dripSound != null)
        {
            AudioManager.Instance.PlaySFXRandomPitch(_dripSound, 0.2f, 0.7f, 1.3f);
        }
    }
}