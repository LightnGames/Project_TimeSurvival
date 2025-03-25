using UnityEngine;

public class Shotgun : Weapon
{
    private enum PumpState
    {
        NeedPump,
        ShotReady,
    }

    [SerializeField] private Transform _pumpMeshTransform;
    [SerializeField] private Transform _pumpDefaultAncherTransform;
    private Vector3 _pumpDefaultLocalPosition = Vector3.zero;
    private Quaternion _pumpDefaultRotation = Quaternion.identity;
    private event CatchableItem.VibrateEvent _pumpVibrationEvent = null;
    private event CatchableItem.XrHandAnimationTransformEvent _pumpAnimationTransformEvent = null;
    private PumpState _pumpState;
    private Vector3 _pumpInversePosition;
    private Quaternion _pumpInverseRotation;
    private Quaternion _pumpCatchedInverseRotation;

    protected override void Awake()
    {
        base.Awake();

        _pumpDefaultLocalPosition = _pumpMeshTransform.localPosition;
        _pumpDefaultRotation = _pumpMeshTransform.localRotation;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private bool IsPumpCatched()
    {
        return _pumpAnimationTransformEvent != null;
    }

    public void PumpCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform pumpTransform)
    {
        // メイングリップをつかんでいないときはポンプを動かさない
        if (!IsMainGripCatched())
        {
            Quaternion rotationOffset = pumpTransform.rotation* _pumpInverseRotation * _pumpCatchedInverseRotation;
            Vector3 positionOffset = (pumpTransform.rotation * _pumpInverseRotation) * _pumpInversePosition;
            transform.SetPositionAndRotation(pumpTransform.position+positionOffset, rotationOffset);
            return;
        }

        float pumpLimitRange = -0.1f;
        float pumpMovementHeight = Vector3.Dot(_pumpMeshTransform.forward, pumpTransform.position - _pumpDefaultAncherTransform.position);
        // ポンプされているか
        if (pumpMovementHeight < pumpLimitRange && IsReadyToShotTimer())
        {
            if (_pumpState == PumpState.NeedPump)
            {
                _pumpState = PumpState.ShotReady;
                _pumpVibrationEvent(0.7f, 0.5f, 0.1f);
            }
        }

        float pumpMovementClampedHeight = Mathf.Clamp(pumpMovementHeight, pumpLimitRange, 0.0f);
        _pumpMeshTransform.localPosition = _pumpDefaultLocalPosition + Vector3.forward * pumpMovementClampedHeight;

        transform.rotation = Quaternion.LookRotation(pumpTransform.position - transform.position);

        _pumpInversePosition = transform.position - pumpTransform.position;
        _pumpInverseRotation = Quaternion.Inverse(pumpTransform.rotation);
        _pumpCatchedInverseRotation = transform.rotation;
    }

    public override void MainGripCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform mainGripTransform)
    {
        base.MainGripCatchedUpdate(input, mainGripTransform);

        if (IsPumpCatched())
        {
            transform.position = mainGripTransform.position;
            return;
        }
        transform.SetPositionAndRotation(mainGripTransform.position, mainGripTransform.rotation);
    }

    protected override bool CanShot()
    {
        return base.CanShot() && _pumpState == PumpState.ShotReady;
    }

    protected override void Shot()
    {
        base.Shot();

        _pumpState = PumpState.NeedPump;
    }

    public override void MainGripCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        base.MainGripCatched(vibrateEvent, transformEvent);
    }

    public override void MainGripReleased()
    {
        base.MainGripReleased();
        CheckReleaseWeapon();
    }

    public void PumpCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        _pumpVibrationEvent = vibrateEvent;
        _pumpAnimationTransformEvent = transformEvent;
    }

    public void PumpReleased()
    {
        _pumpMeshTransform.localPosition = _pumpDefaultLocalPosition;
        _pumpMeshTransform.localRotation = _pumpDefaultRotation;
        _pumpVibrationEvent = null;
        _pumpAnimationTransformEvent = null;
        CheckReleaseWeapon();
    }

    private void CheckReleaseWeapon()
    {
        if(IsPumpCatched() || IsMainGripCatched())
        {
            return;
        }

        ReleasedWeapon();
    }
}
