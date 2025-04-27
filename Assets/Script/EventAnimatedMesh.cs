using UnityEngine;

public class EventAnimatedMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] Animation _breakAnimation;
    [SerializeField] AudioClip _audioClip;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void OnEventTriggered()
    { 
        _breakAnimation.Play();
        _audioSource.PlayOneShot(_audioClip);
    }
}
