using UnityEngine;

public class Weapon : GrabableItem
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzleFlashAncher;
    private readonly int TriggerHash = Animator.StringToHash("Trigger");
    private readonly int ShotHash = Animator.StringToHash("Shot");

    protected override void Awake()
    {
        base.Awake();
    }

    public override void CatchedUpdate(in GrabableItemInputData input)
    {
        base.CatchedUpdate(input);
        _animator.SetFloat(TriggerHash, input._indexTrigger);
    }

    public override void OnIndexTriggered() 
    {
        _animator.SetTrigger(ShotHash);
    }
}
