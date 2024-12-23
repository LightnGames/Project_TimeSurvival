using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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
    private LinkedList<GrabableItem> _catchableItems;
    private GrabableItem _catchingItem;
    private Coroutine _catchTransformAnimationCoroutine = null;

    private void Awake()
    {
        _catchableItems = new LinkedList<GrabableItem>();
        _xrHand.OnTriggerEnterEvent += OnTriggerEnterEvent;
    }
    private void Update()
    {
        var inputTarget = _handType == HandType.Left ? OVRInput.RawAxis1D.LHandTrigger : OVRInput.RawAxis1D.RHandTrigger;
        float grabAmount = OVRInput.Get(inputTarget);
        _xrHand.HandAnimator.SetFloat(GrabHash, grabAmount, 0.1f, Time.deltaTime);


        const float GrabThrehold = 0.7f;
        if (grabAmount > GrabThrehold)
        {
            TryToCatchItem();
        }
        else
        {
            TryToReleaseItem();
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
        _catchingItem.transform.parent = _xrHand.HandTransformAncher;
        _catchTransformAnimationCoroutine = StartCoroutine(PlayCatchTransformAnimation());
        _catchingItem.Catched();
    }

    private void TryToReleaseItem()
    {
        if (_catchingItem == null)
        {
            return;
        }

        _catchingItem.transform.parent = null;
        _catchingItem.Released();
        _catchingItem = null;
    }

    private IEnumerator PlayCatchTransformAnimation()
    {
        Transform itemTransform = _catchingItem.transform;
        Vector3 startPosition = itemTransform.localPosition;
        Quaternion startRotation = itemTransform.rotation;
        const float AnimationLength = 0.1f;
        float animationTime = 0.0f;
        while (animationTime < 1.0f)
        {
            Vector3 position = Vector3.Lerp(startPosition, Vector3.zero, animationTime);
            Quaternion rotation = Quaternion.Lerp(startRotation, Quaternion.identity, animationTime);
            itemTransform.SetLocalPositionAndRotation(position, rotation);
            animationTime += Time.deltaTime / AnimationLength;
            yield return null;
        }
        itemTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _catchTransformAnimationCoroutine = null;
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
}
