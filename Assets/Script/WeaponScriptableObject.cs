using UnityEngine;

[CreateAssetMenu(menuName = "WeaponScriptableObject")]
public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField] private int _maxAmmo;
    [SerializeField] private ParticleSystem _muzzleFlashParticle;
    [SerializeField] private ParticleSystem _impactParticle;
    [SerializeField] private MeshRenderer _bulletTrailCilinderMeshRenderer;
    [SerializeField] private AnimationCurve _recoilPitchEuler;
    [SerializeField] private AnimationCurve _recoilTranslationZ;
    [SerializeField] private float _recoilTimeInSec;

    public int MaxAmmo { get { return _maxAmmo; } }
    public ParticleSystem MuzzleFlashParticlePrefab { get {  return _muzzleFlashParticle; } }
    public ParticleSystem ImpactParticlePrefab { get { return _impactParticle; } }
    public MeshRenderer BulletTrailCilinderMeshRendererPrefab { get { return _bulletTrailCilinderMeshRenderer; } }
    public AnimationCurve RecoilPitchEuler { get {  return _recoilPitchEuler; } }
    public AnimationCurve RecoilTranslationZ { get { return _recoilTranslationZ; } }
    public float RecoilTimeInSec { get { return _recoilTimeInSec; } }
}
