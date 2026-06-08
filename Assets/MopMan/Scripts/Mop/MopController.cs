using UnityEngine;
using UnityEngine.InputSystem;

public class MopController : MonoBehaviour
{
    [Header("VR input")]
    [Tooltip("Assign the right-hand Activate action from XRI Default Input Actions.")]
    public InputActionReference vrTriggerAction;

    [Header("Desktop input (editor testing only)")]
    public Key actionKey = Key.Space;
    public bool useMouse = true;

    [HideInInspector] public MopSlot currentSlot;

    void OnEnable()  => vrTriggerAction?.action.Enable();
    void OnDisable() => vrTriggerAction?.action.Disable();

    void Update()
    {
        if (currentSlot == null) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool pressed = (vrTriggerAction != null && vrTriggerAction.action.WasPerformedThisFrame())
                       || (kb != null && kb[actionKey].wasPressedThisFrame)
                       || (useMouse && mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (pressed) currentSlot.Interact(this);
    }
}
