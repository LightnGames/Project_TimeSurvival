using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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
    PlayerControlInput _input;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {

    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerControlInput();
        _input.Enable();
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
        var inputMoveVec2 = _input.Player.Move.ReadValue<Vector2>();
        float inputRawLength = inputMoveVec2.magnitude;
        float inputLength = Mathf.Min(inputRawLength, 1.0f);
        Vector2 inputDirection = inputMoveVec2 / inputRawLength;

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

        // Note: カメラ向いてるほうに移動を試すやつ
        // スマホ版はこれでよい
#if APP_MODE_ANDROID_STAND_ALONE
        float moveSpeed = 1.0f;
        Vector3 cameraSpaceMoveDirection = Camera.main.transform.TransformVector(new Vector3(inputDirection.x, 0, inputDirection.y));
        cameraSpaceMoveDirection.y = 0.0f;
        cameraSpaceMoveDirection = cameraSpaceMoveDirection.normalized;

        Vector3 moveDirection = Vector3.zero;
        if (inputLength > 0.001f)
        {
            moveDirection = cameraSpaceMoveDirection * inputLength;
        }
        float invTimeScale = 1.0f / Time.timeScale;
        float moveLength = moveSpeed * invTimeScale * Time.deltaTime;

        moveDirection += Vector3.down;
        _characterController.Move(moveDirection * moveLength);
#else
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
#endif
        _moveLengthFromFootStepStart += moveLength * inputLength;

        if (_moveLengthFromFootStepStart > _playerScriptableObject.FootStepRateInMeeter)
        {
            int footStepIndex = Random.Range(0, _playerScriptableObject.FootStepConcreteAudioClips.Length);
            _audioSource.PlayOneShot(_playerScriptableObject.FootStepConcreteAudioClips[footStepIndex]);
            _moveLengthFromFootStepStart -= _playerScriptableObject.FootStepRateInMeeter;
        }

        UpdateCameraLook();
    }

    private void UpdateCameraLook() {
        var inputVec2 = _input.Player.Camera.ReadValue<Vector2>();
        float x = inputVec2.x;
        float y = inputVec2.y;
                
        gameObject.transform.Rotate(Vector3.up, x);
        _headTransfrom.Rotate(Vector3.left, y);
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
