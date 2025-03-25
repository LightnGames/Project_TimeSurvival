using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class CatchableItem : MonoBehaviour
{
    [SerializeField] private Transform _catchAncherTransform;
    [SerializeField] private int _grabItemIndex = 0;
    [SerializeField] private bool _unuseLocalOffset = false;
    [SerializeField] private bool _unuseCatchedAnimation = false;
    private Vector3 _defaultLocalPosition;
    private Quaternion _defaultLocalRotation;

    public class GrabableItemInputData
    {
        public float _indexTrigger;
    }

    private ParentConstraint _constraints;
    public delegate void VibrateEvent(float frequency, float amplitude, float duration);
    public delegate void XrHandAnimationTransformEvent(Vector3 localPosition, Quaternion localRotation);
    protected event VibrateEvent _vibrationEvent = null;
    protected event XrHandAnimationTransformEvent _xrHandAnimationTransformEvent = null;
    public int GrabItemIndex { get { return _grabItemIndex; } }
    public Transform CatchAncherTransform { get { return _catchAncherTransform; } }
    public bool IsCatched { get { return _vibrationEvent != null; } }

    protected virtual void Awake()
    {
        _constraints = GetComponent<ParentConstraint>();
    }

    public virtual void CatchedUpdate(in GrabableItemInputData input) { }

    public virtual void OnIndexTriggered() { }

    public void Catched(VibrateEvent vibrateEvent, XrHandAnimationTransformEvent transformEvent, in ConstraintSource constraintSource)
    {
        transform.GetLocalPositionAndRotation(out _defaultLocalPosition, out _defaultLocalRotation);
        _constraints.constraintActive = true;
        _constraints.AddSource(constraintSource);
        if (!_unuseLocalOffset)
        {
            _constraints.SetTranslationOffset(0, CatchAncherTransform.localPosition);
        }
        OnCatched(vibrateEvent, transformEvent);
    }

    protected virtual void OnCatched(VibrateEvent vibrateEvent, XrHandAnimationTransformEvent transformEvent)
    {
        _vibrationEvent = vibrateEvent;
        _xrHandAnimationTransformEvent = transformEvent;
    }

    public virtual void Released()
    {
        _constraints.constraintActive = false;
        _constraints.RemoveSource(0);
        _vibrationEvent = null;
        _xrHandAnimationTransformEvent = null;
        transform.SetLocalPositionAndRotation(_defaultLocalPosition, _defaultLocalRotation);
    }

    public IEnumerator PlayCatchTransformAnimation()
    {
        if (_unuseCatchedAnimation)
        {
            _constraints.weight = 1.0f;
            yield break;
        }

        const float AnimationLength = 0.1f;
        float animationTime = 0.0f;
        while (animationTime < 1.0f)
        {
            _constraints.weight = animationTime;
            animationTime += Time.deltaTime / AnimationLength;
            yield return null;
        }
        _constraints.weight = 1.0f;
    }
}
