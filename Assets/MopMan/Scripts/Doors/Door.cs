using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public GameObject doorObject; 
    public Vector3 openOffset = new Vector3(0, 3, 0); // Distance and direction to move
    public float smoothSpeed = 5f; // How fast the door slides

    private Vector3 targetPosition;
    private Vector3 closedPosition;

    void Start()
    {
        // Save the starting position as the closed state
        closedPosition = doorObject.transform.localPosition;
        targetPosition = closedPosition;
    }

    void Update()
    {
        // Smoothly interpolate the position toward the target
        doorObject.transform.localPosition = Vector3.Lerp(
            doorObject.transform.localPosition, 
            targetPosition, 
            Time.deltaTime * smoothSpeed
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            // Open position is the start position plus the offset
            targetPosition = closedPosition + openOffset;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            targetPosition = closedPosition;
        }
    }
}

