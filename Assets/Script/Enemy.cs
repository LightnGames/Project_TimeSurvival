using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

static public class EnemyUtil
{
    static public IEnumerator FitKilledPlayerTransform(Transform transform, float enemyToPlayerDistance = 1.0f)
    {
        Transform playerCameraTransform = GameSceneManager.Instance.CameraTransform;
        Transform playerTransform = GameSceneManager.Instance.PlayerTransform;
        Quaternion startRotation = transform.rotation;

        Vector3 startPosition = transform.position;
        float animationTime = 0.0f;
        float animationLength = 0.1f;
        while (true)
        {
            Vector3 playerCameraPositionXZ = playerCameraTransform.position;
            playerCameraPositionXZ.y = transform.position.y;

            Vector3 playerPositionXZ = playerTransform.position;
            playerPositionXZ.y = transform.position.y;

            Vector3 cameraToPlayerOffset = (transform.position - playerCameraPositionXZ).normalized;
            Vector3 endPosition = playerPositionXZ + cameraToPlayerOffset * enemyToPlayerDistance;
            Vector3 position = Vector3.Lerp(startPosition, endPosition, animationTime);

            Quaternion endRotation = Quaternion.LookRotation(playerPositionXZ - transform.position);
            Quaternion rotation = Quaternion.Lerp(startRotation, endRotation, animationTime);
            transform.SetPositionAndRotation(position, rotation);

            if (animationTime >= 1.0f)
            {
                break;
            }
            animationTime = Mathf.Min(Time.deltaTime / Time.timeScale / animationLength, 1.0f);
            yield return null;
        }
    }
}

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
    private readonly int DeadHash = Animator.StringToHash("Dead");
    private readonly int DamageHash = Animator.StringToHash("Damage");
    private readonly int KillPlayerHash = Animator.StringToHash("KillPlayer");
    private int _health = 0;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.enabled = false;
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

    public void PlayOneShotFenceRampage()
    {
        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomFenceRampageAudioClip());
    }

    public void PlayOneShotFootL()
    {
        _footAudioSourceL.PlayOneShot(_enemyScriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotFootR()
    {
        _footAudioSourceR.PlayOneShot(_enemyScriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotBodyRip()
    {
        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomBodyRipAudioClip());
    }

    public void EndSpawn()
    {
        _navMeshAgent.enabled = true;
    }

    private void Update()
    {
        if (!_navMeshAgent.enabled)
        {
            return;
        }

        if (GameSceneManager.Instance.IsGameOver)
        {
            _navMeshAgent.enabled = false;
            return;
        }

        _navMeshAgent.destination = GameSceneManager.Instance.PlayerTransform.position;
        _navMeshAgent.nextPosition = transform.position;
        bool isWalking = _navMeshAgent.velocity.sqrMagnitude > 0.05f;
        _animator.SetBool(IsWalingHash, isWalking);

        if (Vector3.Distance(_navMeshAgent.destination, transform.position) < _navMeshAgent.stoppingDistance + 0.2f)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        _animator.SetTrigger(KillPlayerHash);
        IDamageable damageable = GameSceneManager.Instance.PlayerTransform.GetComponent<IDamageable>();
        damageable.Damage(1, transform);
        _navMeshAgent.enabled = false;
        StartCoroutine(EnemyUtil.FitKilledPlayerTransform(transform));
    }

    public void Damage(int damageAmount, Transform damageSource)
    {
        if (IsDead())
        {
            return;
        }

        _health = Mathf.Max(_health - damageAmount, 0);
        if (_health == 0)
        {
            _animator.SetTrigger(DeadHash);
            _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomDeadAudioClip());
            _navMeshAgent.enabled = false;
            return;
        }

        // ダメージ量が体力の半分を超えていたら必ずダメージリアクションする。
        if (damageAmount > _enemyScriptableObject.MaxHealth / 2)
        {
            _animator.SetTrigger(DamageHash);
        }

        _voiceAudioSource.PlayOneShot(_enemyScriptableObject.GetRandomTakeDamageAudioClip());
    }

    public bool IsDead()
    {
        return _health == 0;
    }
}
