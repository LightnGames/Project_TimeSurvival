using System.Collections;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IEventTrigger
{
    [SerializeField] Rigidbody[] _fractureRigidBodies;
    [SerializeField] MeshRenderer _dummyWallMeshRenderer;

    public void OnEventTriggered()
    {
        StartCoroutine(HideDummyWall());

        Vector3 forceVelocity = -transform.right * 200.0f;
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
