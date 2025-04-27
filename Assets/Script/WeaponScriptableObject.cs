using UnityEngine;

//[CreateAssetMenu(menuName = "WeaponScriptableObject")]
public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField] private float _baseDamage;
    [SerializeField] private int _maxAmmo;
    [SerializeField] private float _shotVibrationScale;
    [SerializeField] private ParticleSystem _muzzleFlashParticle;
    [SerializeField] private ParticleSystem _impactParticle;
    [SerializeField] private ParticleSystem _bloodImpactParticle;
    [SerializeField] private Rigidbody _emptyShellPrefab;
    [SerializeField] private MeshRenderer _bulletTrailCilinderMeshRenderer;
    [SerializeField] private AnimationCurve _recoilPitchEuler;
    [SerializeField] private AnimationCurve _recoilTranslationZ;
    [SerializeField] private float _recoilTimeInSec;
    [SerializeField] private AudioClip[] _shotAudioClips;
    [SerializeField] private AudioClip[] _dryFireAudioClips;
    [SerializeField] private AudioClip[] _equipAudioClips;

    public float BaseDamage { get { return _baseDamage; } }
    public float ShotVibrationScale { get { return _shotVibrationScale; } }
    public int MaxAmmo { get { return _maxAmmo; } }
    public ParticleSystem MuzzleFlashParticlePrefab { get {  return _muzzleFlashParticle; } }
    public ParticleSystem ImpactParticlePrefab { get { return _impactParticle; } }
    public ParticleSystem BloodImpactParticlePrefab { get { return _bloodImpactParticle; } }
    public Rigidbody EmptyShellPrefab { get { return _emptyShellPrefab; } }
    public MeshRenderer BulletTrailCilinderMeshRendererPrefab { get { return _bulletTrailCilinderMeshRenderer; } }
    public AnimationCurve RecoilPitchEuler { get {  return _recoilPitchEuler; } }
    public AnimationCurve RecoilTranslationZ { get { return _recoilTranslationZ; } }
    public float RecoilTimeInSec { get { return _recoilTimeInSec; } }
    public AudioClip[] ShotAudioClips { get {  return _shotAudioClips; } }
    public AudioClip[] DryFireAudioClips { get { return _dryFireAudioClips; } } 
    public AudioClip[] EquipAudioClips { get { return _equipAudioClips; } }
}
