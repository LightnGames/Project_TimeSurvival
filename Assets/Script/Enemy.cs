using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable, IEventTrigger
{
    [SerializeField] private int _spawnType;
    [SerializeField] private EnemyScriptableObject _enemyScriptableObject;
    [SerializeField] private AudioSource _footAudioSourceL;
    [SerializeField] private AudioSource _footAudioSourceR;
    [SerializeField] private AudioSource _voiceAudioSource;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    private readonly int IsWalingHash = Animator.StringToHash("Walk");
    private readonly int SpawnTypeHash = Animator.StringToHash("SpawnType");
    private int _health = 0;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _health = _enemyScriptableObject.MaxHealth;
        _animator.SetInteger(SpawnTypeHash, _spawnType);
    }

    public void OnEventTriggered()
    {
        _animator.SetInteger(SpawnTypeHash, 0);
    }

    public void PlayOneShotVoice()
    {
        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomIntimidationAudioClip());
    }

    public void PlayOneShotSmallImpact()
    {
        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomSmallImpactAudioClip());
    }

    public void PlayOneShotFootL()
    {
        _footAudioSourceL.PlayOneShot(_enemyScriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotFootR()
    {
        _footAudioSourceR.PlayOneShot(_enemyScriptableObject.GetRandomFootStepAudioClip());
    }

    public void EndSpawn()
    {
        _navMeshAgent.SetDestination(-Vector3.forward * 3);
    }

    private void Update()
    {
        if (_navMeshAgent.enabled)
        {
            _navMeshAgent.nextPosition = transform.position;
            bool isWalking = _navMeshAgent.velocity.sqrMagnitude > 0.05f;
            _animator.SetBool(IsWalingHash, isWalking);
        }
    }

    public void Damage(int damageAmount)
    {
        if (IsDead())
        {
            return;
        }

        _health = Mathf.Max(_health - damageAmount, 0);
        if (_health == 0)
        {
            _animator.SetTrigger("Dead");
            _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomDeadAudioClip());
            _navMeshAgent.enabled = false;
            return;
        }

        _animator.SetTrigger("Damage");
        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomTakeDamageAudioClip());
    }

    public bool IsDead()
    {
        return _health == 0;
    }
}
