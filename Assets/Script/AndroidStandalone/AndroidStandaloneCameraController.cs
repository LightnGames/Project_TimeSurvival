using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class AndroidStandaloneCameraController : MonoBehaviour
{
    private Vector2 _input = Vector2.zero;
    private void Awake()
    {
#if !APP_MODE_ANDROID_STAND_ALONE
        enabled = false;
        return;
#endif
    }

    private void Start()
    {
        GetComponent<Camera>().fieldOfView = 60;
        TouchInputSystem.Get()._moveEvent += UpdateInputEvent;
        TouchInputSystem.Get()._endEvent += EndInputEvent;
    }

    void EndInputEvent(TouchInputSystem input)
    {
    }

    void UpdateInputEvent(TouchInputSystem input)
    {
        bool invalidTouchZone = input.GetCurrentInputPosition().x < TouchInputSystem.ResolutionSourceRateHeight / 2.0f;
        bool notMoveTouched = input.GetInputMoveLength() < 0.01f;
        if (invalidTouchZone || notMoveTouched)
        {
            return;
        }

        float rotateSpeed = 20.0f;
        float invTimeScale = 1.0f / Time.timeScale;
        Vector2 inputDirection = input.GetInputMoveVector() * Time.deltaTime * invTimeScale * rotateSpeed;
        _input.x = _input.x + inputDirection.x;
        _input.y = _input.y + inputDirection.y;

        Quaternion yawQuaternion = Quaternion.AngleAxis(_input.x, Vector3.up);
        Quaternion pitchQuaternion = Quaternion.AngleAxis(_input.y, -Vector3.right);
        transform.localRotation = yawQuaternion * pitchQuaternion;
    }
}
