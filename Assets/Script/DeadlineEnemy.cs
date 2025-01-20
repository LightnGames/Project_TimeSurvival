using UnityEngine;

public class DeadlineEnemy : MonoBehaviour
{
    private Animator _animator;
    private readonly int DeadlinePositionZId = Shader.PropertyToID("_DeadlinePositionZ");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.speed = (1.0f / Time.timeScale) * 0.25f;
        Shader.SetGlobalFloat(DeadlinePositionZId, transform.position.z);
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(DeadlinePositionZId, -5.0f);
    }
}
