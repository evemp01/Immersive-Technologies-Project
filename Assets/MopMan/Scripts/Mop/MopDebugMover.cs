using UnityEngine;
using UnityEngine.InputSystem;

// Desktop testing only: drive the mop with the keyboard so scrubbing can be tried without VR.
// Arrows slide it on the floor, R/F raise and lower it. Disable this for the VR build.
public class MopDebugMover : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float verticalSpeed = 1f;

    void Awake()
    {
        // Go kinematic so physics doesn't fight the manual movement.
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        Vector3 horizontal = Vector3.zero;
        if (kb.upArrowKey.isPressed) horizontal.z += 1f;
        if (kb.downArrowKey.isPressed) horizontal.z -= 1f;
        if (kb.rightArrowKey.isPressed) horizontal.x += 1f;
        if (kb.leftArrowKey.isPressed) horizontal.x -= 1f;
        horizontal = Vector3.ClampMagnitude(horizontal, 1f) * moveSpeed;

        float vy = 0f;
        if (kb.rKey.isPressed) vy += 1f;
        if (kb.fKey.isPressed) vy -= 1f;
        vy *= verticalSpeed;

        transform.position += (horizontal + Vector3.up * vy) * Time.deltaTime;
    }
}
