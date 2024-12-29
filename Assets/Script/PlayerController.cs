using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        stick += OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
#if UNITY_EDITOR
        stick.x = Input.GetAxis("Horizontal");
        stick.y = Input.GetAxis("Vertical");
#endif
        float inputLength = Mathf.Min(stick.magnitude, 1.0f);
        if (inputLength < 0.1f)
        {
            return;
        }

        float moveSpeed = 1.0f;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = stick.x / inputLength;
        moveDirection.z = stick.y / inputLength;
        _characterController.Move(moveDirection * inputLength * moveSpeed * Time.deltaTime);
    }
}
