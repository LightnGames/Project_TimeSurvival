using System.Collections;
using UnityEngine;

public class BreakableMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] Rigidbody[] _fractureRigidBodies;
    [SerializeField] MeshRenderer _dummyWallMeshRenderer;
    [SerializeField] float _forceScale;

    public void OnEventTriggered()
    {
        StartCoroutine(HideDummyWall());

        Vector3 forceVelocity = transform.forward * _forceScale;
        foreach (var rigidBody in _fractureRigidBodies)
        {
            rigidBody.isKinematic = false;
            rigidBody.AddForce(forceVelocity);
        }
    }

    private IEnumerator HideDummyWall()
    {
        for(int i = 0; i < 3; ++i)
        {
            yield return null;
        }

        _dummyWallMeshRenderer.enabled = false;
    }
}
