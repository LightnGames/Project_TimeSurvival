using UnityEngine;

public class WeaponMainGrip : CatchableItem
{
    [SerializeField] private Weapon _weapon;

    protected override void Awake() {
        base.Awake();
    }

    protected override void OnCatched(VibrateEvent vibrateEvent, XrHandHapticEvent haptipicEvent, XrHandAnimationTransformEvent transformEvent) {
        base.OnCatched(vibrateEvent, haptipicEvent, transformEvent);
        _weapon.MainGripCatched(vibrateEvent, haptipicEvent, transformEvent);
    }

    public override void Released() {
        base.Released();
        _weapon.MainGripReleased();
    }

    public override void CatchedUpdate(in GrabableItemInputData input) {
        base.CatchedUpdate(input);
        _weapon.MainGripCatchedUpdate(input, transform);
    }
    public override void OnIndexTriggered() {
        base.OnIndexTriggered();
        _weapon.OnMainGripIndexTriggered();
    }
    public override bool IsCatcheable() {
        return !_weapon.IsEmptyAmmo();
    }
    protected override void OnCatchedAnimationCompleted() {
        _weapon.OnCatchedAnimationCompleted();
    }
}
