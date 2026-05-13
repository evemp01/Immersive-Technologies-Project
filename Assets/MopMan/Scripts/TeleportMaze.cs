using UnityEngine;
using Ubiq.Avatars;
public class Teleport : MonoBehaviour
{
    public Transform target;
    void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {
            other.transform.root.position = target.position;
        }
    }

}
