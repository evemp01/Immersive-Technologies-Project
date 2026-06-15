using UnityEngine;
using UnityEngine.InputSystem;

public class MopController : MonoBehaviour
{
    [Header("VR input")]
    [Tooltip("Assign the right-hand Activate action from XRI Default Input Actions.")]
    public InputActionReference vrTriggerAction;

    [Header("Desktop input (editor testing only)")]
    public Key actionKey = Key.R;

    [HideInInspector] public MopSlot currentSlot;

    void OnEnable()  => vrTriggerAction?.action.Enable();
    void OnDisable() => vrTriggerAction?.action.Disable();

    void Update()
    {
        if (currentSlot == null) return;

        var kb = Keyboard.current;
        bool pressed = (vrTriggerAction != null && vrTriggerAction.action.WasPerformedThisFrame())
                       || (kb != null && kb[actionKey].wasPressedThisFrame);

        if (pressed) currentSlot.Interact(this);
    }
}
