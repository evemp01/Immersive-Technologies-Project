using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]

public class TeleportingSound : MonoBehaviour
{

    [Header("Audio")]
    public AudioClip TeleportSound;
    [Tooltip("Where to play the sound")]
    public Transform target;
    public float volume = 0.15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            AudioSource.PlayClipAtPoint(TeleportSound, target.position, volume);
        }
    }
}

