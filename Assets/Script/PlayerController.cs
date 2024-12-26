using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Texture _globalBentNormalTexture;
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        Shader.SetGlobalTexture("_GlobalBentNormalMap", _globalBentNormalTexture);
    }

    private void Update()
    {
        Vector2 stickL = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
#if UNITY_EDITOR
        stickL.x = Input.GetAxis("Horizontal");
        stickL.y = Input.GetAxis("Vertical");
#endif
        float inputLength = stickL.magnitude;
        if (inputLength < 0.1f)
        {
            return;
        }

        float moveSpeed = 1.0f;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = stickL.x / inputLength;
        moveDirection.z = stickL.y / inputLength;
        _characterController.Move(moveDirection * inputLength * moveSpeed * Time.deltaTime);
    }
}
