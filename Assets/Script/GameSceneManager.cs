using UnityEngine;
using UnityEngine.Audio;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    private void Awake()
    {
    }

    private void OnDestroy()
    {
    }

    private void LateUpdate()
    {
        _audioMixer.SetFloat("Pitch", Time.timeScale);
    }
}
