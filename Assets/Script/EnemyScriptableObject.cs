using UnityEngine;

[CreateAssetMenu(menuName = "EnemyScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    [SerializeField] private int _maxHealth;
    [SerializeField] private AudioClip[] _footStepAudioClips;
    [SerializeField] private AudioClip[] _intimidationAudioClips;
    [SerializeField] private AudioClip[] _takeDamageAudioClips;
    [SerializeField] private AudioClip[] _deadAudioClips;
    [SerializeField] private AudioClip[] _smallImpactAudioClips;
    [SerializeField] private AudioClip[] _fenceRampageAudioClips;

    public int MaxHealth { get { return _maxHealth; } }
    public AudioClip[] FootStepAudioClips { get { return _footStepAudioClips; } }
    public AudioClip[] IntimidationAudioClips { get { return _intimidationAudioClips; } }
    public AudioClip[] TakeDamageAudioClips { get { return _takeDamageAudioClips; } }
    public AudioClip[] DeadAudioClips { get { return _takeDamageAudioClips; } }
    public AudioClip[] SmallImpactAudioClips { get { return _smallImpactAudioClips; } }
    public AudioClip[] FenceRampageAudioClips { get { return _fenceRampageAudioClips; } }

    public AudioClip GetRandomFootStepAudioClip()
    {
        return FootStepAudioClips[Random.Range(0, FootStepAudioClips.Length)];
    }

    public AudioClip GetRandomIntimidationAudioClip()
    {
        return IntimidationAudioClips[Random.Range(0, IntimidationAudioClips.Length)];
    }

    public AudioClip GetRandomTakeDamageAudioClip()
    {
        return TakeDamageAudioClips[Random.Range(0, TakeDamageAudioClips.Length)];
    }

    public AudioClip GetRandomDeadAudioClip()
    {
        return DeadAudioClips[Random.Range(0, DeadAudioClips.Length)];
    }

    public AudioClip GetRandomSmallImpactAudioClip()
    {
        return SmallImpactAudioClips[Random.Range(0, SmallImpactAudioClips.Length)];
    }

    public AudioClip GetRandomFenceRampageAudioClip()
    {
        return FenceRampageAudioClips[Random.Range(0, FenceRampageAudioClips.Length)];
    }
}
