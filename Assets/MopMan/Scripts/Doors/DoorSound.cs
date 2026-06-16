using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]


public class DoorSound : MonoBehaviour
{
    [Header("Door Reference")]
    public LockedDoor targetDoor;

    [Header("Audio")]
    public AudioClip OpeningSound;
    public AudioClip ClosingSound;
    public float volume = 0.3f;
    private AudioSource audioSource;

    void Start()
    {
        // Fetch the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;

        if (targetDoor == null)
        {
            targetDoor = GetComponent<LockedDoor>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            // Play sound when the door opens
            if (targetDoor == null)
            {
                audioSource.PlayOneShot(OpeningSound, volume);
            }
            else if (targetDoor.isUnlocked == true)
            {
                audioSource.PlayOneShot(OpeningSound, volume);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            if (targetDoor == null)
            {
                audioSource.PlayOneShot(ClosingSound);
            }
            else if (targetDoor.isUnlocked == true)
            {
                audioSource.PlayOneShot(ClosingSound);
            }
        }
    }
}

