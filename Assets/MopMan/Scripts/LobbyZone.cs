using UnityEngine;

// Trigger covering the lobby. Reports the local player's presence to the
// GameManager, which combines it with the other client's to detect the win.
[RequireComponent(typeof(Collider))]
public class LobbyZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other)) GameManager.Instance?.SetLocalInLobby(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other)) GameManager.Instance?.SetLocalInLobby(false);
    }

    static bool IsPlayer(Collider other) =>
        other.CompareTag("Player") || other.CompareTag("DesktopPlayer");
}
