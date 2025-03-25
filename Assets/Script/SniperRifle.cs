using UnityEngine;

public class SniperRifle : Weapon
{
    private enum PumpState
    {
        NeedBoltAction,
        ShotReady,
    }

    [SerializeField] private Transform _boltMeshTransform;
    [SerializeField] private Transform _boltDefaultAncherTransform;
    [SerializeField] private Camera _scopeCamera;
    private Vector3 _boltDefaultLocalPosition = Vector3.zero;
    private event CatchableItem.VibrateEvent _boltVibrationEvent = null;
    private event CatchableItem.XrHandAnimationTransformEvent _boltAnimationTransformEvent = null;
    private event CatchableItem.VibrateEvent _subGripVibrationEvent = null;
    private event CatchableItem.XrHandAnimationTransformEvent _subGripAnimationTransformEvent = null;
    private PumpState _pumpState;
    private Vector3 _subGripInversePosition;
    private Quaternion _subGripInverseRotation;
    private Quaternion _subGripCatchedInverseRotation;

    protected override void Awake()
    {
        base.Awake();

        _boltDefaultLocalPosition = _boltMeshTransform.localPosition;
        _scopeCamera.enabled = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private bool IsBoltCatched()
    {
        return _boltAnimationTransformEvent != null;
    }

    private bool IsSubGripCatched()
    {
        return _subGripAnimationTransformEvent != null;
    }

    public override void MainGripCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        base.MainGripCatched(vibrateEvent, transformEvent);
        _scopeCamera.enabled = true;
    }

    public override void MainGripCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform mainGripTransform)
    {
        base.MainGripCatchedUpdate(input, mainGripTransform);

        if (IsSubGripCatched())
        {
            transform.position = mainGripTransform.position;
            return;
        }
        transform.SetPositionAndRotation(mainGripTransform.position, mainGripTransform.rotation);
    }

    public override void MainGripReleased()
    {
        base.MainGripReleased();
        _scopeCamera.enabled = false;
        CheckReleaseWeapon();
    }

    public void BoltCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        _boltVibrationEvent = vibrateEvent;
        _boltAnimationTransformEvent = transformEvent;
    }

    public void BoltReleased()
    {
        _boltMeshTransform.localPosition = _boltDefaultLocalPosition;
        _boltVibrationEvent = null;
        _boltAnimationTransformEvent = null;
    }

    public void SubGripCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        _subGripVibrationEvent = vibrateEvent;
        _subGripAnimationTransformEvent = transformEvent;
    }

    public void SubGripReleased()
    {
        _subGripVibrationEvent = null;
        _subGripAnimationTransformEvent = null;
        CheckReleaseWeapon();
    }

    private void CheckReleaseWeapon()
    {
        if (IsMainGripCatched() || IsSubGripCatched())
        {
            return;
        }

        ReleasedWeapon();
    }

    public void BoltCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform boltTransform)
    {
        float pumpLimitRange = -0.1f;
        float pumpMovementHeight = Vector3.Dot(_boltMeshTransform.forward, boltTransform.position - _boltDefaultAncherTransform.position);
        if (pumpMovementHeight < pumpLimitRange && IsReadyToShotTimer())
        {
            if (_pumpState == PumpState.NeedBoltAction)
            {
                _pumpState = PumpState.ShotReady;
                _boltVibrationEvent(0.7f, 0.5f, 0.1f);
            }
        }

        float pumpMovementClampedHeight = Mathf.Clamp(pumpMovementHeight, pumpLimitRange, 0.0f);
        _boltMeshTransform.localPosition = _boltDefaultLocalPosition + Vector3.forward * pumpMovementClampedHeight;
    }

    public void SubGripCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform subGripTransform)
    {
        if (!IsMainGripCatched())
        {
            Quaternion rotationOffset = subGripTransform.rotation * _subGripInverseRotation * _subGripCatchedInverseRotation;
            Vector3 positionOffset = (subGripTransform.rotation * _subGripInverseRotation) * _subGripInversePosition;
            transform.SetPositionAndRotation(subGripTransform.position + positionOffset, rotationOffset);
            return;
        }

        transform.rotation = Quaternion.LookRotation(subGripTransform.position - transform.position);
        _subGripInversePosition = transform.position - subGripTransform.position;
        _subGripInverseRotation = Quaternion.Inverse(subGripTransform.rotation);
        _subGripCatchedInverseRotation = transform.rotation;
    }

    protected override bool CanShot()
    {
        return base.CanShot() && _pumpState == PumpState.ShotReady;
    }

    protected override void Shot()
    {
        base.Shot();

        _pumpState = PumpState.NeedBoltAction;
    }
}
