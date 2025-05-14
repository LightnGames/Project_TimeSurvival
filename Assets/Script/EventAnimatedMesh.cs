using UnityEngine;

public class EventAnimatedMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] MeshRenderer _dummyMeshRenderer;
    [SerializeField] MeshRenderer _animatedMeshRender;
    [SerializeField] Animation _breakAnimation;
    [SerializeField] AudioClip _audioClip;
    [SerializeField] AudioClip[] _idleAudioClips;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_dummyMeshRenderer != null)
        {
            _animatedMeshRender.enabled = false;
            _dummyMeshRenderer.enabled = true;
        }
    }

    public void OnEventTriggered()
    {
        if (_dummyMeshRenderer != null)
        {
            _animatedMeshRender.enabled = true;
            _dummyMeshRenderer.enabled = false;
        }
        _breakAnimation.Play();
        _audioSource.PlayOneShot(_audioClip);
    }
}
