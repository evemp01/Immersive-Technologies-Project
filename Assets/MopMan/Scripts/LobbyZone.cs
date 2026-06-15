using UnityEngine;

// Reports the local player's lobby presence to the GameManager. Uses a position
// check instead of trigger events, which are not raised reliably when the player
// is teleported in and out of the zone.
[RequireComponent(typeof(Collider))]
public class LobbyZone : MonoBehaviour
{
    public float checkInterval = 0.3f;

    private Collider zone;
    private bool inside;
    private float timer;

    void Awake() => zone = GetComponent<Collider>();

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;

        Camera cam = Camera.main;
        bool now = cam != null && zone.bounds.Contains(cam.transform.position);
        if (now != inside)
        {
            inside = now;
            GameManager.Instance?.SetLocalInLobby(now);
        }
    }
}
