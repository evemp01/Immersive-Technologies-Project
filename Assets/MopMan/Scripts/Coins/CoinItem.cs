using System.Collections;
using UnityEngine;

public class CoinItem : MonoBehaviour
{
    [HideInInspector] public string id;

    public float respawnDelay = 10f;

    void Start()
    {
        id = transform.position.ToString();
        CoinManager.Instance.Register(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.Collect(id, respawnDelay);
        }
    }
}
