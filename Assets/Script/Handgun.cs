using UnityEngine;

public class Handgun : Weapon
{

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    protected override bool IsReadyToShot()
    {
        return base.IsReadyToShot();
    }

    protected override void Shot()
    {
        base.Shot();
    }

    public override void MainGripCatchedUpdate(in CatchableItem.GrabableItemInputData input, Transform mainGripTransform)
    {
        base.MainGripCatchedUpdate(input, mainGripTransform);
        transform.SetLocalPositionAndRotation(mainGripTransform.position, mainGripTransform.rotation);
    }

    public override void MainGripCatched(CatchableItem.VibrateEvent vibrateEvent, CatchableItem.XrHandAnimationTransformEvent transformEvent)
    {
        base.MainGripCatched(vibrateEvent, transformEvent);
        CatchedWeapon();
    }

    public override void MainGripReleased()
    {
        base.MainGripReleased();
        ReleasedWeapon();
    }
}
