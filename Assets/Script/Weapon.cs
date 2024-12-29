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
        ParticleSystem particleSystem = Instantiate(_weaponScriptableObject.muzzleFlashParticle, _muzzleFlashAncher.transform.position, _muzzleFlashAncher.transform.rotation);
        Destroy(particleSystem.gameObject, 3.0f);
        Vibrate(0, 1, 0.1f);
    }
}
