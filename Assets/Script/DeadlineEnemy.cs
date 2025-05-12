using UnityEngine;

public class DeadlineEnemy : MonoBehaviour
{
    [SerializeField] private DeadlineEnemyScriptableObject _scriptableObject;
    [SerializeField] private AudioSource _footAudioSourceL;
    [SerializeField] private AudioSource _footAudioSourceR;
    private Renderer[] _renderers;
    private Animator _animator;
    private readonly int DeadlinePositionZId = Shader.PropertyToID("_DeadlinePositionZ");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        SwitchMeshVisibility(false);
    }

    private void Update()
    {
        _animator.speed = (1.0f / Time.timeScale);
        Shader.SetGlobalFloat(DeadlinePositionZId, transform.position.z);
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(DeadlinePositionZId, -10.0f);
    }

    public void PlayOneShotFootL()
    {
        _footAudioSourceL.PlayOneShot(_scriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotFootR()
    {
        _footAudioSourceR.PlayOneShot(_scriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotSmallImpact()
    {
        _footAudioSourceL.PlayOneShot(_scriptableObject.GetRandomSmallImpactAudioClip());
    }

    private void SwitchMeshVisibility(bool visibility)
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = visibility;
        }
    }

    public void Spawn()
    {
        SwitchMeshVisibility(true);
        _animator.enabled = true;
    }
}
