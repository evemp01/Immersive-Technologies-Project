using UnityEngine;

public class MotionDetection : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource; // Internal reference to the player
    //public AudioClip MovingObjectAudioClip; // Changed from AudioSource to AudioClip
    

    private Vector3 lastPosition;

    void Start()
    {
        // Get or add the AudioSource component automatically
        audioSource.loop = true; // Usually a good idea for continuous footstep loops
        // audioSource.clip = MovingObjectAudioClip; // Assign the footstep clip to the AudioSource

        lastPosition = transform.position;
    }

    void Update()
    {
        
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved > 0 && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (distanceMoved == 0 && audioSource.isPlaying)
        {
            audioSource.Pause();
        }

        lastPosition = transform.position;
    }
}