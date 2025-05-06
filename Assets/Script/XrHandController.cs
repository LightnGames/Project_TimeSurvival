using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Animations;

public class XrHandController : MonoBehaviour
{
    public enum HandType
    {
        Left,
        Right,
    };

    [SerializeField] private HandType _handType;
    [SerializeField] private XrHandEventRepeater _xrHand;

    private readonly int GrabHash = Animator.StringToHash("Grab");
    private readonly int TriggerHash = Animator.StringToHash("Trigger");
    private readonly int GrabItemIndexHash = Animator.StringToHash("GrabItemIndex");
    private LinkedList<CatchableItem> _catchableItems;
    private CatchableItem _catchingItem;
    private Coroutine _catchTransformAnimationCoroutine = null;
    private float _prevIndexTriggerAmount = 0.0f;
    private ConstraintSource _constraintSource;

    private void Awake()
    {
        _catchableItems = new LinkedList<CatchableItem>();
        _xrHand.OnTriggerEnterEvent += OnTriggerEnterEvent;

        _constraintSource = new ConstraintSource();
        _constraintSource.sourceTransform = _xrHand.HandTransformAncher;
        _constraintSource.weight = 1.0f;
    }
    private void LateUpdate()
    {
        var handTriggerTarget = _handType == HandType.Left ? OVRInput.RawAxis1D.LHandTrigger : OVRInput.RawAxis1D.RHandTrigger;
        var indexTriggerTarget = _handType == HandType.Left ? OVRInput.RawAxis1D.LIndexTrigger : OVRInput.RawAxis1D.RIndexTrigger;
        float grabAmount = OVRInput.Get(handTriggerTarget);
        float grabIndexAmount = OVRInput.Get(indexTriggerTarget);

        CatchableItem.GrabableItemInputData _grabableItemInput = new CatchableItem.GrabableItemInputData();
        _grabableItemInput._indexTrigger = grabIndexAmount;

        float invTimeScale = 1.0f / Time.timeScale;
        _xrHand.HandAnimator.speed = invTimeScale;
        _xrHand.HandAnimator.SetFloat(GrabHash, grabAmount, 0.1f, Time.deltaTime * invTimeScale);
        _xrHand.HandAnimator.SetFloat(TriggerHash, grabIndexAmount);

        const float GrabThrehold = 0.7f;
        if (grabAmount > GrabThrehold)
        {
            TryToCatchItem();
        }
        else
        {
            TryToReleaseItem();
        }

        if(_catchingItem != null)
        {
            const float GrabIndexThreshold = 0.9f;
            if (grabIndexAmount > GrabIndexThreshold && _prevIndexTriggerAmount < GrabIndexThreshold)
            {
                _catchingItem.OnIndexTriggered();
            }

            _catchingItem.CatchedUpdate(_grabableItemInput);
            _prevIndexTriggerAmount = grabIndexAmount;
        }

#if UNITY_EDITOR
        foreach (var item in _catchableItems)
        {
            Debug.DrawLine(item.transform.position, item.transform.position + item.transform.up * 0.2f, item.IsCatched ? Color.red : Color.yellow);
        }
#endif
    }

    private void TryToCatchItem()
    {
        if (_catchableItems.Count == 0 || _catchingItem != null)
        {
            return;
        }

        CatchableItem nearestItem = null;
        float minSqrLength = float.MaxValue;
        foreach (var item in _catchableItems)
        {
            if (item.IsCatched)
            {
                continue;
            }

            if (!item.IsCatcheable())
            {
                continue;
            }

            float sqrLength = Vector3.SqrMagnitude(_xrHand.HandTransformAncher.position - item.transform.position);
            if (minSqrLength > sqrLength)
            {
                nearestItem = item;
                minSqrLength = sqrLength;
            }
        }

        if (nearestItem == null)
        {
            return;
        }

        _catchingItem = nearestItem;
        if(_catchTransformAnimationCoroutine != null)
        {
            StopCoroutine(_catchTransformAnimationCoroutine);
        }
        _xrHand.HandAnimator.SetInteger(GrabItemIndexHash, _catchingItem.GrabItemIndex);
        _catchingItem.Catched(OnXrHandVibrated, OnXrHandTransformAnimated, _constraintSource);
        _catchTransformAnimationCoroutine = StartCoroutine(PlayCatchTransformAnimation());
    }

    private void TryToReleaseItem()
    {
        if (_catchingItem == null)
        {
            return;
        }

        _xrHand.HandAnimator.SetInteger(GrabItemIndexHash, 0);
        _catchingItem.Released();
        _catchingItem = null;
    }

    private IEnumerator PlayCatchTransformAnimation()
    {
        yield return StartCoroutine(_catchingItem.PlayCatchTransformAnimation());
        OnXrHandVibrated(0.5f, 0.2f, 0.1f);
        _catchTransformAnimationCoroutine = null;
    }

    private void OnTriggerEnterEvent(CatchableItem item, bool enter)
    {
        if (enter)
        {
            _catchableItems.AddLast(item);
        }
        else
        {
            _catchableItems.Remove(item);
        }
    }

    private void OnXrHandVibrated(float frequency, float amplitude, float duration)
    {
        OVRInput.Controller controller = _handType == HandType.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        StartCoroutine(VibrateXrHand(frequency, amplitude, duration, controller));
    }

    private IEnumerator VibrateXrHand(float frequency, float amplitude, float duration, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    private void OnXrHandTransformAnimated(Vector3 localPosition, Quaternion localRotation)
    {
        _xrHand.transform.SetLocalPositionAndRotation(localPosition, localRotation);
    }
}
