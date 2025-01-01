using UnityEngine;

public class Weapon : GrabableItem
{
    [SerializeField] private WeaponScriptableObject _weaponScriptableObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzleFlashAncher;
    [SerializeField] private Transform _triggerTransform;
    private readonly int ShotHash = Animator.StringToHash("Shot");
    private float _triggerAngle = 0.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void LateUpdate()
    {
        _triggerTransform.localRotation = Quaternion.Euler(_triggerAngle, 0, 0);
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);

        const float MaxTriggerAngle = 45.0f;
        _triggerAngle = input._indexTrigger * MaxTriggerAngle;
    }

    public override void OnIndexTriggered() 
    {
        _animator.SetTrigger(ShotHash);
        ParticleSystem muzzleFlashParticle = Instantiate(_weaponScriptableObject.MuzzleFlashParticlePrefab, _muzzleFlashAncher.transform.position, _muzzleFlashAncher.transform.rotation);
        Destroy(muzzleFlashParticle.gameObject, 3.0f);
        Vibrate(0, 1, 0.1f);

        const float MaxRayLength = 30.0f;
        RaycastHit hit;
        Vector3 p1 = _muzzleFlashAncher.position;
        Vector3 p2 = p1 + _muzzleFlashAncher.forward * MaxRayLength;
        if (Physics.CapsuleCast(p1, p2, 0.05f, transform.forward, out hit))
        {
            ParticleSystem impactParticle = Instantiate(_weaponScriptableObject.ImpactParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactParticle.gameObject, 10.0f);
        }
    }
}
