using System.Collections;
using UnityEngine;

public class BreakableMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] Rigidbody[] _fractureRigidBodies;
    [SerializeField] MeshRenderer[] _dummyWallMeshRenderers;
    [SerializeField] float _forceScale;
    [SerializeField] AudioClip _audioClip;

    private AudioSource _audioSource;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void OnEventTriggered()
    {
        StartCoroutine(HideDummyWall());

        Vector3 forceVelocity = -transform.up * _forceScale;
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

        foreach(MeshRenderer meshRenderer in _dummyWallMeshRenderers)
        {
            meshRenderer.enabled = false;
        }

        _audioSource.PlayOneShot(_audioClip);
    }
}
