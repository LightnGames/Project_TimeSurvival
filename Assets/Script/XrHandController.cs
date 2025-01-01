using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

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
    private LinkedList<GrabableItem> _catchableItems;
    private GrabableItem _catchingItem;
    private Coroutine _catchTransformAnimationCoroutine = null;
    private float _prevIndexTriggerAmount = 0.0f;

    private void Awake()
    {
        _catchableItems = new LinkedList<GrabableItem>();
        _xrHand.OnTriggerEnterEvent += OnTriggerEnterEvent;
    }
    private void Update()
    {
        var handTriggerTarget = _handType == HandType.Left ? OVRInput.RawAxis1D.LHandTrigger : OVRInput.RawAxis1D.RHandTrigger;
        var indexTriggerTarget = _handType == HandType.Left ? OVRInput.RawAxis1D.LIndexTrigger : OVRInput.RawAxis1D.RIndexTrigger;
        float grabAmount = OVRInput.Get(handTriggerTarget);
        float grabIndexAmount = OVRInput.Get(indexTriggerTarget);

        GrabableItem.GrabableItemInputData _grabableItemInput = new GrabableItem.GrabableItemInputData();
        _grabableItemInput._indexTrigger = grabIndexAmount;

        _xrHand.HandAnimator.SetFloat(GrabHash, grabAmount, 0.1f, Time.deltaTime);
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
    }

    private void TryToCatchItem()
    {
        if (_catchableItems.Count == 0 || _catchingItem != null)
        {
            return;
        }

        GrabableItem nearestItem = null;
        float minSqrLength = float.MaxValue;
        foreach (var item in _catchableItems)
        {
            if (item.IsCatched)
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
        _catchingItem.transform.parent = _xrHand.HandTransformAncher;
        _catchTransformAnimationCoroutine = StartCoroutine(PlayCatchTransformAnimation());
        _catchingItem.Catched(OnVibrate);
    }

    private void TryToReleaseItem()
    {
        if (_catchingItem == null)
        {
            return;
        }

        _xrHand.HandAnimator.SetInteger(GrabItemIndexHash, 0);
        _catchingItem.transform.parent = null;
        _catchingItem.Released();
        _catchingItem = null;
    }

    private IEnumerator PlayCatchTransformAnimation()
    {
        Transform itemTransform = _catchingItem.transform;
        Vector3 startPosition = itemTransform.localPosition;
        Quaternion startRotation = itemTransform.rotation;
        Vector3 endPosition = _catchingItem.CatchAncherTransform.localPosition;
        const float AnimationLength = 0.1f;
        float animationTime = 0.0f;
        while (animationTime < 1.0f)
        {
            Vector3 position = Vector3.Lerp(startPosition, endPosition, animationTime);
            Quaternion rotation = Quaternion.Lerp(startRotation, Quaternion.identity, animationTime);
            itemTransform.SetLocalPositionAndRotation(position, rotation);
            animationTime += Time.deltaTime / AnimationLength;
            yield return null;
        }
        itemTransform.SetLocalPositionAndRotation(endPosition, Quaternion.identity);
        _catchTransformAnimationCoroutine = null;

        OnVibrate(0.5f, 0.3f, 0.1f);
    }

    private void OnTriggerEnterEvent(GrabableItem item, bool enter)
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

    private void OnVibrate(float frequency, float amplitude, float duration)
    {
        OVRInput.Controller controller = _handType == HandType.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        StartCoroutine(Vibrate(frequency, amplitude, duration, controller));
    }

    private IEnumerator Vibrate(float frequency, float amplitude, float duration, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
}
