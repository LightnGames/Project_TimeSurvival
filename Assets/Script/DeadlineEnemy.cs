using UnityEngine;
using UnityEngine.AI;

public class DeadlineEnemy : MonoBehaviour
{
    [SerializeField] private DeadlineEnemyScriptableObject _scriptableObject;
    [SerializeField] private AudioSource _footAudioSourceL;
    [SerializeField] private AudioSource _footAudioSourceR;
    private Renderer[] _renderers;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private readonly int DeadlinePositionZId = Shader.PropertyToID("_DeadlinePositionZ");
    private readonly int KillPlayerHash = Animator.StringToHash("KillPlayer");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.enabled = false;
        SwitchMeshVisibility(false);
    }

    private void Update()
    {
        _animator.speed = (1.0f / Time.timeScale);
        Shader.SetGlobalFloat(DeadlinePositionZId, transform.position.z);

        if (!_navMeshAgent.enabled)
        {
            return;
        }

        if (GameSceneManager.Instance.IsGameOver)
        {
            return;
        }

        _navMeshAgent.destination = GameSceneManager.Instance.PlayerTransform.position;
        _navMeshAgent.nextPosition = transform.position;

        if (Vector3.Distance(_navMeshAgent.destination, transform.position) < _navMeshAgent.stoppingDistance + 1.0f)
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
        StartCoroutine(EnemyUtil.FitKilledPlayerTransform(transform, 1.5f));
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(DeadlinePositionZId, -10.0f);
    }

    public void PlayOneShotFootL()
    {
        _footAudioSourceL.PlayOneShot(_scriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotFootR()
    {
        _footAudioSourceR.PlayOneShot(_scriptableObject.GetRandomFootStepAudioClip());
    }

    public void PlayOneShotSmallImpact()
    {
        _footAudioSourceL.PlayOneShot(_scriptableObject.GetRandomSmallImpactAudioClip());
    }

    private void SwitchMeshVisibility(bool visibility)
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = visibility;
        }
    }

    public void Spawn()
    {
        SwitchMeshVisibility(true);
        _animator.enabled = true;
        _navMeshAgent.enabled = true;
    }
}
