using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerScriptableObject _playerScriptableObject;
    [SerializeField] Renderer _fadeRenderer;
    private readonly int FadeId = Shader.PropertyToID("_Fade");
    private readonly int GlobalTimeScaleId = Shader.PropertyToID("_GlobalTimeScale");
    private CharacterController _characterController;
    private AudioSource _audioSource;
    private float _moveLengthFromFootStepStart = 0.0f;
    private Material[] _fadeMaterials;
    private float _prevFade = 0.0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _fadeMaterials = new Material[_fadeRenderer.sharedMaterials.Length];
        for(int i = 0; i < _fadeMaterials.Length; i++)
        {
            _fadeMaterials[i] = _fadeRenderer.materials[i];
            _fadeRenderer.sharedMaterials[i] = _fadeMaterials[i];
        }
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

        UpdateFade(inputLength);

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

    private void UpdateFade(float inputLength)
    {
        float fade = _prevFade + (inputLength - _prevFade) / _playerScriptableObject.FadeTime;
        foreach (Material material in _fadeMaterials)
        {
            float remapedFade = _playerScriptableObject.GetRemapedFade(fade);
            material.SetFloat(FadeId, remapedFade);
        }

        _prevFade = fade;

        Time.timeScale = _playerScriptableObject.GetRemapedTimeScale(fade);
        Shader.SetGlobalFloat(GlobalTimeScaleId, 1.0f - fade);
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(GlobalTimeScaleId, 0.0f);
    }
}
