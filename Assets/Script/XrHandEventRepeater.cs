using System;
using UnityEngine;

public class XrHandEventRepeater : MonoBehaviour
{
    [SerializeField] private Transform _handTransformAncher;

    public Animator HandAnimator { get; private set; }
    public Transform HandTransformAncher { get { return _handTransformAncher; } }
    public Action<CatchableItem, bool> OnTriggerEnterEvent { get; set; }

    private void Awake()
    {
        HandAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CatchableItem item = other.gameObject.GetComponent<CatchableItem>();
        if (item == null)
        {
            return;
        }

        if(OnTriggerEnterEvent != null)
        {
            OnTriggerEnterEvent(item, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CatchableItem item = other.gameObject.GetComponent<CatchableItem>();
        if (item == null)
        {
            return;
        }

        if (OnTriggerEnterEvent != null)
        {
            OnTriggerEnterEvent(item, false);
        }
    }
}
