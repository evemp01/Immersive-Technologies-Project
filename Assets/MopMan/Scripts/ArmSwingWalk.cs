using UnityEngine;
using UnityEngine.InputSystem;

public class ArmSwingWalk : MonoBehaviour
{
    [Header("VR References")]
    public CharacterController characterController;
    public Transform xrOrigin;
    public Transform leftController;
    public Transform rightController;
    public Transform mainCamera;

    [Header("Physic limits")]
    public float acceleration    = 20f;
    public float drag            = 5f;
    public float maxSpeed        = 3f;
    public float minMovementThreshold = 0.005f;

    [Header("Alternation")]
    [Tooltip("Wait time in seconds before oscillation reset")]
    public float idleResetTime = 2f;

    [Header("Primary Button")]
    public bool requirePrimaryButton = true;
    public InputActionProperty leftPrimaryButton;
    public InputActionProperty rightPrimaryButton;

    private float prevLeftY;
    private float prevRightY;
    private bool  prevLeftMoving;
    private bool  prevRightMoving;

    private enum Turn { Either, Left, Right }
    private Turn nextTurn        = Turn.Either;
    private bool leftSwingActive  = false;
    private bool rightSwingActive = false;
    private float idleTimer       = 0f;

    private Vector3 horizontalVelocity;
    private float   verticalVelocity;

    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        prevLeftY  = xrOrigin.InverseTransformPoint(leftController.position).y;
        prevRightY = xrOrigin.InverseTransformPoint(rightController.position).y;
    }

    void OnEnable()
    {
        leftPrimaryButton.action.Enable();
        rightPrimaryButton.action.Enable();
    }

    void OnDisable()
    {
        leftPrimaryButton.action.Disable();
        rightPrimaryButton.action.Disable();
    }

    void Update()
    {
        float currentLeftY  = xrOrigin.InverseTransformPoint(leftController.position).y;
        float currentRightY = xrOrigin.InverseTransformPoint(rightController.position).y;

        float leftDelta  = Mathf.Abs(currentLeftY  - prevLeftY);
        float rightDelta = Mathf.Abs(currentRightY - prevRightY);

        bool leftMoving  = leftDelta  > minMovementThreshold;
        bool rightMoving = rightDelta > minMovementThreshold;

        //If no one is moving, we reset the swing
        if (!leftMoving && !rightMoving)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleResetTime)
            {
                nextTurn  = Turn.Either;
                idleTimer = 0f;
            }
        }
        else idleTimer = 0f;

        float movementContribution = 0f;

        if (!requirePrimaryButton || IsButtonPressed())
        {
            //Left arm start moving
            if (leftMoving && !prevLeftMoving)
            {
                if (nextTurn == Turn.Either || nextTurn == Turn.Left)
                {
                    leftSwingActive = true;
                    nextTurn = Turn.Right;
                }
                
            }
            //Right arm start moving
            if (rightMoving && !prevRightMoving)
            {
                if (nextTurn == Turn.Either || nextTurn == Turn.Right)
                {
                    rightSwingActive = true;
                    nextTurn = Turn.Left;
                }
            }

            //Arm stopped moving
            if (!leftMoving  && prevLeftMoving)  leftSwingActive  = false;
            if (!rightMoving && prevRightMoving) rightSwingActive = false;

            //Only add current ok oscillation
            if (leftSwingActive)  movementContribution += leftDelta;
            if (rightSwingActive) movementContribution += rightDelta;
        }
        else
        {
            leftSwingActive  = false;
            rightSwingActive = false;
        }

        //Apply movement
        if (movementContribution > 0f)
        {
            Vector3 forward = mainCamera.forward;
            forward.y = 0f;
            forward.Normalize();
            horizontalVelocity += forward * movementContribution * acceleration;
        }

        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, drag * Time.deltaTime);

        verticalVelocity = characterController.isGrounded
            ? -2f
            : verticalVelocity + Physics.gravity.y * Time.deltaTime;

        Vector3 move = horizontalVelocity;
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        //Update prev
        prevLeftY = currentLeftY;
        prevRightY = currentRightY;
        prevLeftMoving = leftMoving;
        prevRightMoving = rightMoving;
    }

    private bool IsButtonPressed()
        => leftPrimaryButton.action.IsPressed() || rightPrimaryButton.action.IsPressed();
}