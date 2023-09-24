using System.Collections;
using UnityEngine;

public class AudioObj : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void Initialize(AudioClip _audioClip)
    {
        _audioSource.clip = _audioClip;
        StartCoroutine(destroyDelay(_audioClip.length));
    }

    private IEnumerator destroyDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}