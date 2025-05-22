using UnityEngine;

public class SniperRifleSubGrip : CatchableItem
{
    [SerializeField] private SniperRifle _weapon;

    protected override void OnCatched(VibrateEvent vibrateEvent, XrHandHapticEvent hapticEvent, XrHandAnimationTransformEvent transformEvent) {
        base.OnCatched(vibrateEvent, hapticEvent, transformEvent);
        _weapon.SubGripCatched(vibrateEvent, transformEvent);
    }

    public override void Released() {
        base.Released();
        _weapon.SubGripReleased();
    }

    public override void CatchedUpdate(in GrabableItemInputData input) {
        base.CatchedUpdate(input);
        _weapon.SubGripCatchedUpdate(input, transform);
    }

    public override bool IsCatcheable() {
        return !_weapon.IsEmptyAmmo() && _weapon.IsMainGripCatched();
    }
}
