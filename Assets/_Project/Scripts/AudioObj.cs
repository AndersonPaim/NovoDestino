using UnityEngine;

public class AudioObj : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void Initialize(AudioClip _audioClip)
    {
        _audioSource.clip = _audioClip;
    }
}