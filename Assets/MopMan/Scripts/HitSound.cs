using UnityEngine;

public class HitSound : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the GameObject containing the MonsterPatrol script into this slot!")]

    public MonsterPatrol monsterPatrolScript;

    [Header("Sound")]
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    public float volume = 1.0f;

    private int previousLivesSnapshot;
    private bool initialized = false;

    void Update()
    {
        // 1. Safety check to make sure we assigned the reference
        if (monsterPatrolScript != null)
        {
            // 2. Read the public playerLives variable directly from the target script
            int currentLives = monsterPatrolScript.playerLives;

            if (!initialized)
            {
                previousLivesSnapshot = currentLives;
                initialized = true;
            }

            if (currentLives < previousLivesSnapshot && currentLives > 0)
            {
                // Play the damage sound effect here
                // For example, if you have an AudioSource component attached to this GameObject:
                AudioSource.PlayClipAtPoint(DamageSound, transform.position, volume);
                previousLivesSnapshot = currentLives; // Update the snapshot after playing the sound
            }

            if (currentLives == 0 && previousLivesSnapshot > 0)
            {
                AudioSource.PlayClipAtPoint(DeathSound, transform.position, volume);
                previousLivesSnapshot = currentLives; // Update the snapshot after playing the sound
            }
        }
    }
}