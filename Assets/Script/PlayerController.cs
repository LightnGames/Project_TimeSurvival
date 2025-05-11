using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerScriptableObject _playerScriptableObject;
    [SerializeField] Renderer _fadeRenderer;
    [SerializeField] Transform _headTransfrom;
    [SerializeField] private AudioSource _audioSource;
    private readonly int FadeId = Shader.PropertyToID("_Fade");
    private readonly int GlobalTimeScaleId = Shader.PropertyToID("_GlobalTimeScale");
    private CharacterController _characterController;
    private float _moveLengthFromFootStepStart = 0.0f;
    private Material[] _fadeMaterials;
    private float _prevFade = 0.0f;
    private bool _dead = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {

    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _fadeMaterials = new Material[_fadeRenderer.sharedMaterials.Length];
        for (int i = 0; i < _fadeMaterials.Length; i++)
        {
            _fadeMaterials[i] = _fadeRenderer.materials[i];
            _fadeRenderer.sharedMaterials[i] = _fadeMaterials[i];
        }

#if UNITY_EDITOR || APP_MODE_ANDROID_STAND_ALONE
        _headTransfrom.localPosition = Vector3.up * 1.6f;
        Application.targetFrameRate = 60;
#endif
        print("Game Started");
    }

    private void Update()
    {
        Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        stick += OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
#if UNITY_EDITOR
        stick.x = Input.GetAxisRaw("Horizontal");
        stick.y = Input.GetAxisRaw("Vertical");
#endif
        float inputRawLength = stick.magnitude;
        float inputLength = Mathf.Min(inputRawLength, 1.0f);
        Vector2 inputDirection = stick / inputRawLength;

#if APP_MODE_ANDROID_STAND_ALONE
        TouchPadController touchPadController = TouchPadController.Get();
        inputLength = touchPadController.TouchPadInputAmount;
        inputDirection = touchPadController.TouchPadDirection;
#endif

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

        float moveSpeed = 1.0f;
        Vector3 moveDirection = Vector3.down;
        if (inputLength > 0.001f)
        {
            moveDirection.x = inputDirection.x * inputLength;
            moveDirection.z = inputDirection.y * inputLength;
        }
        float invTimeScale = 1.0f / Time.timeScale;
        float moveLength = moveSpeed * invTimeScale * Time.deltaTime;
        _characterController.Move(moveDirection * moveLength);
        _moveLengthFromFootStepStart += moveLength * inputLength;

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
