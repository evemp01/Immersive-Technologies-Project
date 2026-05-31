using UnityEngine;
using UnityEngine.InputSystem;

// Lets the desktop player use the shelf: while standing in a MopSlot's zone, the action key
// (or click) buys/equips that mop. Cleaning the floor is the physical mop's job (MopScrubber).
public class MopController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Key that buys/equips the mop you're standing at.")]
    public Key actionKey = Key.Space;

    [Tooltip("Also trigger with the left mouse button.")]
    public bool useMouse = true;

    // Set by MopSlot while the player is inside its trigger zone.
    [HideInInspector] public MopSlot currentSlot;

    void Update()
    {
        if (currentSlot == null) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool pressed = (kb != null && kb[actionKey].wasPressedThisFrame)
                       || (useMouse && mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (pressed) currentSlot.Interact(this);
    }
}
