using UnityEngine;
using UnityEngine.InputSystem;

public class TouchPadController : MonoBehaviour
{
    [SerializeField] private float _touchPadLimitLength = 100;
    [SerializeField] private RectTransform _touchPadRectTransform;

    private static TouchPadController _touchPadController;
    private RectTransform _rectTransform;
    private Vector2 _touchPadDirection = Vector2.zero;
    private float _touchPadInputAmount = 0.0f;

    public Vector2 TouchPadDirection { get { return _touchPadDirection; } }
    public float TouchPadInputAmount { get { return _touchPadInputAmount; } }

    private void Awake()
    {
#if !APP_MODE_ANDROID_STAND_ALONE
        Destroy(transform.parent.gameObject);
        return;
#endif
        _rectTransform = GetComponent<RectTransform>();
        _touchPadController = this;
    }

    private void LateUpdate()
    {
        TouchInputSystem touchInputSystem = TouchInputSystem.Get();
        bool invalidTouchZone = touchInputSystem.GetCurrentInputPosition().x > TouchInputSystem.ResolutionSourceRateHeight / 2.0f;
        bool noTouched = touchInputSystem.GetCurrentRawInputPosition().sqrMagnitude < 0.01f;
        if (invalidTouchZone || noTouched)
        {
            _touchPadDirection = Vector2.zero;
            _touchPadInputAmount = 0.0f;
            _touchPadRectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        Vector2 localpoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, touchInputSystem.GetCurrentRawInputPosition(), null, out localpoint);

        Vector2 touchPadVector = localpoint;
        float touchPadInputRawLength = touchPadVector.magnitude;
        Vector2 touchPadDirection = touchPadVector / touchPadInputRawLength;
        float touchPadInputClampedLength = Mathf.Min(touchPadInputRawLength, _touchPadLimitLength);

        _touchPadDirection = touchPadDirection;
        _touchPadInputAmount = touchPadInputClampedLength / _touchPadLimitLength;
        _touchPadRectTransform.anchoredPosition =  touchPadDirection * touchPadInputClampedLength;
    }

    private void OnDestroy()
    {
        _touchPadController = null;
    }

    static public TouchPadController Get()
    {
        return _touchPadController;
    }
}
