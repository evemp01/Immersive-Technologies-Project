using UnityEngine;
using Ubiq.Avatars;

public class TeleportLobby : MonoBehaviour
{
    // Creiamo un menu a tendina per l'Inspector
    public enum TeleportRole { ToMazePlayer, ToDesktopPlayer }

    [Header("Teleport Settings")]
    public Transform target;
    
    [Tooltip("Choose the role assigned to the player using this teleport")]
    public TeleportRole roleToAssign;

    void OnTriggerEnter(Collider other)
    {   
        // Permettiamo il teletrasporto a chiunque sia un giocatore (anche se ha già cambiato ruolo)
        if (other.CompareTag("Player") || other.CompareTag("DesktopPlayer") || other.CompareTag("MainCamera"))
        {
            Transform playerRoot = other.transform.root;

            // 1. ASSEGNAZIONE DINAMICA DEL RUOLO
            if (roleToAssign == TeleportRole.ToMazePlayer)
            {
                playerRoot.tag = "Player";
                
                // Per sicurezza con la VR, taggiamo anche l'oggetto specifico che ha toccato il trigger
                other.gameObject.tag = "Player"; 
            }
            else if (roleToAssign == TeleportRole.ToDesktopPlayer)
            {
                playerRoot.tag = "DesktopPlayer";
                other.gameObject.tag = "DesktopPlayer";
            }

            // 2. TELETRASPORTO SICURO PER VR 
            // (Disabilitare il CharacterController per un istante evita bug di collisione durante il teletrasporto)
            CharacterController cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerRoot.position = target.position;

            playerRoot.position = target.position + new Vector3(-1.5f, 0f, 0f);

            // MODIFICA 2: Orient the player towards the negative X axis
            // Vector3.left is a shortcut for new Vector3(-1, 0, 0)
            playerRoot.rotation = Quaternion.LookRotation(Vector3.right);

            if (cc != null) cc.enabled = true;
            
            Debug.Log("Player teleported and assigned role: " + roleToAssign.ToString());
        }
    }
}