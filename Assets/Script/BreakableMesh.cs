using System.Collections;
using UnityEngine;

public class BreakableMesh : MonoBehaviour, IEventTrigger
{
    [SerializeField] Rigidbody[] _fractureRigidBodies;
    [SerializeField] MeshRenderer[] _dummyWallMeshRenderers;
    [SerializeField] Transform _forceAncher;
    [SerializeField] float _delayTimeInSec = 0.0f;
    [SerializeField] float _forceScale;
    [SerializeField] AudioClip _audioClip;

    private AudioSource _audioSource;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void OnEventTriggered()
    {
        StartCoroutine(PlayEvents());
    }

    private IEnumerator PlayEvents()
    {
        yield return new WaitForSeconds(_delayTimeInSec);

        Vector3 forceVelocity = -transform.up;
        foreach (var rigidBody in _fractureRigidBodies)
        {
            rigidBody.isKinematic = false;

            if (_forceAncher != null)
            {
                forceVelocity = (rigidBody.transform.position - _forceAncher.position).normalized;
            }
            rigidBody.AddForce(forceVelocity * _forceScale);
        }

        for (int i = 0; i < 3; ++i)
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
