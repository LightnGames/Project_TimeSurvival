using UnityEngine;

public class GrabableItem : MonoBehaviour
{
    private Rigidbody _rigidbody;

    public bool IsCatched { get { return _rigidbody.isKinematic; } }

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void Catched()
    {
        _rigidbody.isKinematic = true;
    }

    public virtual void Released()
    {
        _rigidbody.isKinematic = false;
    }
}
