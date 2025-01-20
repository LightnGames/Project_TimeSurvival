using UnityEngine;
using UnityEngine.Audio;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    private float _defaultFixedDeltaTime = 0.0f;

    private void Awake()
    {
        OVRManager.SetSpaceWarp(true);
        OVRPlugin.systemDisplayFrequency = 90.0f;
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnDestroy()
    {
    }

    private void LateUpdate()
    {
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
        _audioMixer.SetFloat("Pitch", Time.timeScale);
    }
}
