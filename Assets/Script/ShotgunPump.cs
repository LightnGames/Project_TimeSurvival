using UnityEngine;

public class ShotgunPump : CatchableItem
{
    [SerializeField] private Shotgun _weapon;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnCatched(VibrateEvent vibrateEvent, XrHandAnimationTransformEvent transformEvent)
    {
        base.OnCatched(vibrateEvent, transformEvent);
        _weapon.PumpCatched(vibrateEvent, transformEvent);
    }

    public override void Released()
    {
        base.Released();
        _weapon.PumpReleased();
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);
        _weapon.PumpCatchedUpdate(input, transform);
    }

    public override bool IsCatcheable()
    {
        return !_weapon.IsEmptyAmmo();
    }
}
