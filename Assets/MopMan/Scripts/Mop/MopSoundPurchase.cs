using UnityEngine;

public class PurchaseSound : MonoBehaviour
{
    [Header("Mop Reference")]
    public MopShelf mopShelf; // Reference to the MopShelf to check for purchases

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip purchaseSound;
    public float volume = 0.15f;
    

    private MopSlot equippedMop;

    void Start()
    {
        // Fetch the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;

        if(mopShelf == null)
        {
            mopShelf = GetComponent<MopShelf>();
        }

        equippedMop = mopShelf.equippedSlot;
    }

    void Update()
    {
        if (mopShelf.equippedSlot != equippedMop)
        {
            audioSource.PlayOneShot(purchaseSound, volume);
        }
        equippedMop = mopShelf.equippedSlot;
    }
}

