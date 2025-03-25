using UnityEngine;

public class WeaponMainGrip : CatchableItem
{
    [SerializeField] private Weapon _weapon;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnCatched(VibrateEvent vibrateEvent, XrHandAnimationTransformEvent transformEvent)
    {
        base.OnCatched(vibrateEvent, transformEvent);
        _weapon.MainGripCatched(vibrateEvent, transformEvent);
    }

    public override void Released()
    {
        base.Released();
        _weapon.MainGripReleased();
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);
        _weapon.MainGripCatchedUpdate(input, transform);
    }

    public override void OnIndexTriggered()
    {
        base.OnIndexTriggered();
        _weapon.OnMainGripIndexTriggered();
    }
}
