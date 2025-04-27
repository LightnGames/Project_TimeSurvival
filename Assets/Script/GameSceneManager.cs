using UnityEngine;
using UnityEngine.Audio;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    private static GameSceneManager _instance;
    private float _defaultFixedDeltaTime = 0.0f;
    private Transform _playerTransform;
    private Transform _cameraTransform;
    private bool _isGameOver = false;

    public static GameSceneManager Instance {  get { return _instance; } }
    public Transform PlayerTransform { get { return _playerTransform; } }
    public Transform CameraTransform { get { return _cameraTransform; } }
    public bool IsGameOver { get { return _isGameOver; } }

    private void Awake()
    {
        OVRManager.SetSpaceWarp(true);

        OVRPlugin.systemDisplayFrequency = 90.0f;
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _cameraTransform = Camera.main.transform;
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void LateUpdate()
    {
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
        _audioMixer.SetFloat("Pitch", Time.timeScale);
    }

    public void GameOver()
    {
        _isGameOver = true;
    }
}
