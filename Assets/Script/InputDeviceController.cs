using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceController : MonoBehaviour
{    
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            InputSystem.DisableDevice(Mouse.current);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        if(Input.GetMouseButtonDown(0)) {
            InputSystem.EnableDevice(Mouse.current);            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }
}
