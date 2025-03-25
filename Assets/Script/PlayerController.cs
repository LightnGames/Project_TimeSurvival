using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerScriptableObject _playerScriptableObject;
    [SerializeField] Renderer _fadeRenderer;
    [SerializeField] Transform _headTransfrom;
    private readonly int FadeId = Shader.PropertyToID("_Fade");
    private readonly int GlobalTimeScaleId = Shader.PropertyToID("_GlobalTimeScale");
    private CharacterController _characterController;
    private AudioSource _audioSource;
    private float _moveLengthFromFootStepStart = 0.0f;
    private Material[] _fadeMaterials;
    private float _prevFade = 0.0f;
    private bool _dead = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _fadeMaterials = new Material[_fadeRenderer.sharedMaterials.Length];
        for (int i = 0; i < _fadeMaterials.Length; i++)
        {
            _fadeMaterials[i] = _fadeRenderer.materials[i];
            _fadeRenderer.sharedMaterials[i] = _fadeMaterials[i];
        }

#if UNITY_EDITOR
        _headTransfrom.localPosition = Vector3.up * 1.6f;
#endif
    }

    private void Update()
    {
        Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        stick += OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
#if UNITY_EDITOR
        stick.x = Input.GetAxisRaw("Horizontal");
        stick.y = Input.GetAxisRaw("Vertical");
#endif
        float inputLength = Mathf.Min(stick.magnitude, 1.0f);

        if (!_dead)
        {
            UpdateFade(inputLength);
        }

        UpdateTimeScale();
        ApplyTimeScaleFade();
        ApplyEnvTimeScaleFade();

        if (_dead)
        {
            return;
        }

        if (inputLength < 0.001f)
        {
            return;
        }

        float moveSpeed = 1.0f;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = stick.x / inputLength;
        moveDirection.z = stick.y / inputLength;
        float invTimeScale = 1.0f / Time.timeScale;
        float moveLength = inputLength * moveSpeed * invTimeScale * Time.deltaTime;
        _characterController.Move(moveDirection * moveLength);
        _moveLengthFromFootStepStart += moveLength;

        if (_moveLengthFromFootStepStart > _playerScriptableObject.FootStepRateInMeeter)
        {
            int footStepIndex = Random.Range(0, _playerScriptableObject.FootStepConcreteAudioClips.Length);
            _audioSource.PlayOneShot(_playerScriptableObject.FootStepConcreteAudioClips[footStepIndex]);
            _moveLengthFromFootStepStart -= _playerScriptableObject.FootStepRateInMeeter;
        }
    }

    private void UpdateFade(float fadeTarget)
    {
        _prevFade = _prevFade + (fadeTarget - _prevFade) / _playerScriptableObject.FadeTime;
    }

    private void ApplyTimeScaleFade()
    {
        float fade = _dead ? 1.0f - _prevFade : _prevFade;
        foreach (Material material in _fadeMaterials)
        {
            float remapedFade = _playerScriptableObject.GetRemapedFade(fade);
            material.SetFloat(FadeId, remapedFade);
        }
    }

    private void ApplyEnvTimeScaleFade()
    {
        Shader.SetGlobalFloat(GlobalTimeScaleId, 1.0f - _prevFade);
    }

    private void UpdateTimeScale()
    {
        Time.timeScale = _playerScriptableObject.GetRemapedTimeScale(_prevFade);
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(GlobalTimeScaleId, 0.0f);
    }

    public void Damage(int damageAmount, Transform damageSource)
    {
        _dead = true;
        StartCoroutine(KilledLookat(damageSource));
        StartCoroutine(RequestForGameRestart());
        GameSceneManager.Instance.GameOver();
    }

    private IEnumerator KilledLookat(Transform sourceTransform)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(sourceTransform.position - transform.position);
        float startFade = _prevFade;
        float animationTime = 0.0f;
        float animationLength = 0.1f;
        while (animationTime < 1.0f)
        {
            _prevFade = Mathf.Lerp(startFade, 1.0f, animationTime);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, animationTime);
            animationTime += Time.deltaTime / Time.timeScale / animationLength;
            yield return null;
        }
    }

    private IEnumerator RequestForGameRestart()
    {
        yield return new WaitForSeconds(6.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
