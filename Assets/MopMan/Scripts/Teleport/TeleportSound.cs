using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]

public class TeleportingSound : MonoBehaviour
{

    [Header("Audio")]
    public AudioClip TeleportSound;
    public float volume = 0.15f;
    private AudioSource audioSource;

    void Start()
    {
        // Fetch the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            audioSource.PlayOneShot(TeleportSound);
        }
    }
}

