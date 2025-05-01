using UnityEngine;

public class EmptyShellAudioController : MonoBehaviour
{
    [SerializeField] AudioClip[] _audioClips;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        int audioIndex = Random.Range(0, _audioClips.Length);
        _audioSource.PlayOneShot(_audioClips[audioIndex]);
    }
}
