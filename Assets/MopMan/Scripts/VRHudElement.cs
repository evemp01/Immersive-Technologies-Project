using UnityEngine;

public class VRHudElement : MonoBehaviour
{
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float horizontalAngle = -20f;
    [SerializeField] private float verticalAngle = 15f; 

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(-verticalAngle, horizontalAngle, 0);
        Vector3 direction = cam.rotation * rotation * Vector3.forward;

        transform.position = cam.position + direction * distanceFromCamera;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}