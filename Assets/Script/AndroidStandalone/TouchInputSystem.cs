using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputSystem : MonoBehaviour {
    private static TouchInputSystem _inputSystem;
    private Vector2 _currentInputPosition = Vector2.zero;
    private Vector2 _prevInputPosition = Vector2.zero;
    private Vector2 _currentRawInputPosition = Vector2.zero;
    private float _inputMoveLength = 0.0f;
    public const float ResolutionSourceRateHeight = 1080.0f;
    public delegate void OnUpdateInput(TouchInputSystem input);
    public event OnUpdateInput _beginEvent;
    public event OnUpdateInput _moveEvent;
    public event OnUpdateInput _endEvent;

    public void Awake() {
        _beginEvent = null;
        _moveEvent = null;
        _endEvent = null;
        _inputSystem = this;
    }

    public Vector2 GetCurrentInputPosition() {
        return _currentInputPosition;
    }
    public Vector2 GetPrevInputPosition() {
        return _prevInputPosition;
    }

    public Vector2 GetCurrentRawInputPosition()
    {
        return _currentRawInputPosition;
    }

    public Vector2 GetInputMoveVector() {
        return _currentInputPosition - _prevInputPosition;
    }

    public float GetInputMoveLength() {
        return _inputMoveLength;
    }

    static public TouchInputSystem Get() {
        return _inputSystem;
    }

    public void Update() {
        _prevInputPosition = _currentInputPosition;

#if UNITY_EDITOR
        Vector3 inputRawPosition = Vector3.zero;
        bool isStart = Input.GetMouseButtonDown(0);
        bool isMove = Input.GetMouseButton(0);
        bool isEnd = Input.GetMouseButtonUp(0);
        if (Input.GetMouseButton(0))
        {
            inputRawPosition = Input.mousePosition;
        }
#else
        Vector3 inputRawPosition = Vector3.zero;
        bool isStart = false;
        bool isMove = false;
        bool isEnd = false;

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            inputRawPosition = touch.position;
            isStart = touch.phase == TouchPhase.Began;
            isMove = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            isEnd = touch.phase == TouchPhase.Ended;
        }
#endif
        _currentRawInputPosition = inputRawPosition;
        float screenScaleRate = ResolutionSourceRateHeight / Screen.width;
        Vector3 inputPosition = inputRawPosition * screenScaleRate;
        _currentInputPosition = new Vector2(inputPosition.x, inputPosition.y);
        _inputMoveLength = (_currentInputPosition - _prevInputPosition).magnitude;

        if (isStart) {
            if (_beginEvent != null)
            {
                _beginEvent(this);
            }
        }

        if (isMove) {
            if (_moveEvent != null)
            {
                _moveEvent(this);
            }
        }

        if (isEnd) {
            if (_endEvent != null)
            {
                _endEvent(this);
            }
        }
    }
}
