using UnityEngine;

public class SniperRifleBolt : CatchableItem
{
    [SerializeField] private SniperRifle _weapon;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnCatched(VibrateEvent vibrateEvent, XrHandAnimationTransformEvent transformEvent)
    {
        base.OnCatched(vibrateEvent, transformEvent);
        _weapon.BoltCatched(vibrateEvent, transformEvent);
    }

    public override void Released()
    {
        base.Released();
        _weapon.BoltReleased();
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);
        _weapon.BoltCatchedUpdate(input, transform);
    }
}
