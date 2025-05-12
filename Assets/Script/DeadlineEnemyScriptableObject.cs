using UnityEngine;

[CreateAssetMenu(menuName = "DeadlineEnemyScriptableObject")]
public class DeadlineEnemyScriptableObject : ScriptableObject
{
    [SerializeField] private AudioClip[] _footStepAudioClips;
    [SerializeField] private AudioClip[] _intimidationAudioClips;
    [SerializeField] private AudioClip[] _takeDamageAudioClips;
    [SerializeField] private AudioClip[] _deadAudioClips;
    [SerializeField] private AudioClip[] _smallImpactAudioClips;

    public AudioClip[] FootStepAudioClips { get { return _footStepAudioClips; } }
    public AudioClip[] IntimidationAudioClips { get { return _intimidationAudioClips; } }
    public AudioClip[] TakeDamageAudioClips { get { return _takeDamageAudioClips; } }
    public AudioClip[] DeadAudioClips { get { return _takeDamageAudioClips; } }
    public AudioClip[] SmallImpactAudioClips { get { return _smallImpactAudioClips; } }

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
}
