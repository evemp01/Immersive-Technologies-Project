using UnityEngine;
using Ubiq.Voip;

public class UbiqVoiceHook : MonoBehaviour
{
    void Start()
    {
        var voipManager = FindObjectOfType<VoipPeerConnectionManager>();
        if (voipManager != null)
        {
            voipManager.OnPeerConnection.AddListener(OnNewConnection);
        }
    }

    void OnNewConnection(VoipPeerConnection connection)
    {
        var crossfadeScript = connection.GetComponentInParent<MonsterCrossfade>();
        if (crossfadeScript == null) crossfadeScript = connection.GetComponentInChildren<MonsterCrossfade>();

        if (crossfadeScript != null)
        {
            var audioSource = connection.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                crossfadeScript.AssignUbiqVoiceSource(audioSource);
            }
        }
    }
}