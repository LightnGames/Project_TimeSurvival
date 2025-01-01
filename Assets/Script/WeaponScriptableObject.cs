using UnityEngine;

[CreateAssetMenu(menuName = "WeaponScriptableObject")]
public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField] private int _maxAmmo;
    [SerializeField] private ParticleSystem _muzzleFlashParticle;
    [SerializeField] private ParticleSystem _impactParticle;

    public int MaxAmmo { get { return _maxAmmo; } }
    public ParticleSystem MuzzleFlashParticlePrefab { get {  return _muzzleFlashParticle; } }
    public ParticleSystem ImpactParticlePrefab { get { return _impactParticle; } }
}
