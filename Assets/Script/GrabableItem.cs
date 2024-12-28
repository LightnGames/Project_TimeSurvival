using UnityEngine;

public abstract class GrabableItem : MonoBehaviour
{
    [SerializeField] private Transform _catchAncherTransform;
    [SerializeField] private int _grabItemIndex = 0;
    private Rigidbody _rigidbody;

    public class GrabableItemInputData
    {
        public float _indexTrigger;
    }


    public int GrabItemIndex { get { return _grabItemIndex; } }
    public Transform CatchAncherTransform { get { return _catchAncherTransform; } }
    public bool IsCatched { get { return _rigidbody.isKinematic; } }

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void CatchedUpdate(in GrabableItemInputData input)
    {
    }

    public virtual void OnIndexTriggered() { }

    public virtual void Catched()
    {
        _rigidbody.isKinematic = true;
    }

    public virtual void Released()
    {
        _rigidbody.isKinematic = false;
    }
}
