using UnityEngine;

//[CreateAssetMenu(menuName = "PlayerScriptableObject")]
public class PlayerScriptableObject : ScriptableObject
{
    [SerializeField] private AudioClip[] _footStepConcreteAudioClips;
    [SerializeField] private float _footStepRateInMeeter;
    [SerializeField] private float _fadeRangeMin;
    [SerializeField] private float _fadeRangeMax;
    [SerializeField] private float _fadeTime;
    [SerializeField] private float _timeScaleRangeMin;
    [SerializeField] private float _timeScaleRangeMax;
    [SerializeField] private float _timeScaleTime;

    public AudioClip[] FootStepConcreteAudioClips { get { return _footStepConcreteAudioClips;} }
    public float FootStepRateInMeeter { get { return _footStepRateInMeeter; } }
    public float FadeRangeMin { get { return _fadeRangeMin; } }
    public float FadeRangeMax { get {  return _fadeRangeMax; } }
    public float FadeTime {  get { return _fadeTime; } }
    public float TimeScaleRangeMin { get { return _timeScaleRangeMin; } }
    public float TimeScaleRangeMax { get { return _timeScaleRangeMax; } }
    public float TimeScaleTime { get { return _timeScaleTime; } }

    public float GetRemapedFade(float rawFade)
    {
       return Mathf.Lerp(FadeRangeMin, FadeRangeMax, rawFade);
    }

    public float GetRemapedTimeScale(float rawTimeScale)
    {
        return Mathf.Lerp(TimeScaleRangeMin, TimeScaleRangeMax, rawTimeScale);
    }
}
