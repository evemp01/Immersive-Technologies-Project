using UnityEngine;
using System.Collections; // Needed for the color fading Coroutine

[RequireComponent(typeof(BoxCollider))]
public class LockedDoor : MonoBehaviour
{
    [Header("Door Movement")]
    public GameObject doorObject; 
    public Vector3 openOffset = new Vector3(0, 3, 0); // Distance and direction to move
    public float smoothSpeed = 5f; // How fast the door slides

    [Header("Key System Integration")]
    public bool isUnlocked = false; 
    public MeshRenderer doorRenderer; // Drag door 3D model here
    public Color lockedFeedbackColor = Color.red; 
    
    private Color originalColor;
    private Vector3 targetPosition;
    private Vector3 closedPosition;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        closedPosition = doorObject.transform.localPosition;
        targetPosition = closedPosition;

        if (doorRenderer != null)
        {
            originalColor = doorRenderer.material.color;
        }
    }

    void Update()
    {
        doorObject.transform.localPosition = Vector3.Lerp(
            doorObject.transform.localPosition, 
            targetPosition, 
            Time.deltaTime * smoothSpeed
        );
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        if (doorRenderer != null) doorRenderer.material.color = Color.green;
    }

    public void LockDoor()
    {
        isUnlocked = false;
        targetPosition = closedPosition;
        if (doorRenderer != null) doorRenderer.material.color = originalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            if (isUnlocked)
            {
                targetPosition = closedPosition + openOffset;
            }
            else
            {
                if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = StartCoroutine(FlashLockedColor());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            targetPosition = closedPosition;
        }
    }

    private IEnumerator FlashLockedColor()
    {
        if (doorRenderer == null) yield break;

        doorRenderer.material.color = lockedFeedbackColor;
        
        yield return new WaitForSeconds(0.5f); 
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            doorRenderer.material.color = Color.Lerp(lockedFeedbackColor, originalColor, t);
            yield return null;
        }
    }
}