using System;
using UnityEngine;

public class XrHandEventRepeater : MonoBehaviour
{
    [SerializeField] private Transform _handTransformAncher;

    public Animator HandAnimator { get; private set; }
    public Transform HandTransformAncher { get { return _handTransformAncher; } }
    public Action<GrabableItem, bool> OnTriggerEnterEvent { get; set; }

    private void Awake()
    {
        HandAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        GrabableItem item = other.gameObject.GetComponent<GrabableItem>();
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
        GrabableItem item = other.gameObject.GetComponent<GrabableItem>();
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
