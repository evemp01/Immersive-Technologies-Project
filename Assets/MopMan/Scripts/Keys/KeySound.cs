using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class KeySound : MonoBehaviour
{

    public AudioClip PickingUpSound;

    public float volume = 0.15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer"))
        {
            // Play sound when the key is picked up
            if (PickingUpSound != null)
            {
                AudioSource.PlayClipAtPoint(PickingUpSound, transform.position, volume);            
            }
        }
    }
}

