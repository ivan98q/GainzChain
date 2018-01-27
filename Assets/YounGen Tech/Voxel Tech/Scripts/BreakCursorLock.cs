using UnityEngine;

public class BreakCursorLock : MonoBehaviour {
    void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}