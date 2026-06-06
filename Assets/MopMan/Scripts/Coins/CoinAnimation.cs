using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    public float rotateSpeed = 90f;
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}