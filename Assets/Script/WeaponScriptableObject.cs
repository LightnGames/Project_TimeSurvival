using UnityEngine;

[CreateAssetMenu(menuName = "WeaponScriptableObject")]
public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField] private int _maxAmmo;
    [SerializeField] private ParticleSystem _muzzleFlashParticle;

    public int MaxAmmo { get { return _maxAmmo; } }
    public ParticleSystem muzzleFlashParticle { get {  return _muzzleFlashParticle; } }
}
