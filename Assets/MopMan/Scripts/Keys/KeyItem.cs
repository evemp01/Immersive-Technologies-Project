using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [HideInInspector] public string id;

    void Start()
    {
        // Automatically uses its exact world position as a network ID
        id = transform.position.ToString(); 
        KeyManager.Instance.Register(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) KeyManager.Instance.Collect(id);
    }
}
