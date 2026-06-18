using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class ScaryLightFlicker : MonoBehaviour
{
    private Light targetLight;
    private float baseIntensity;

    public Renderer sphereRenderer;
    private Material sphereMaterial;

    [Header("Timing")]
    public float minDelay = 20f;
    public float maxDelay = 60f;
    public int minFlashes = 2;
    public int maxFlashes = 5;
    public float minOffTime = 0.15f;
    public float maxOffTime = 0.45f;
    public float minOnTime = 0.15f;
    public float maxOnTime = 0.45f;

    void Start()
    {
        targetLight = GetComponent<Light>();
        baseIntensity = targetLight.intensity;

        if (sphereRenderer != null)
            sphereMaterial = sphereRenderer.material; // Auto-clones the material for this object

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            int numFlashes = Random.Range(minFlashes, maxFlashes + 1);
            for (int i = 0; i < numFlashes; i++)
            {
                SetState(false);
                yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

                SetState(true);
                if (i < numFlashes - 1)
                    yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
            }
        }
    }

    void SetState(bool turnOn)
    {
        targetLight.intensity = turnOn ? baseIntensity : 0f;

        if (sphereMaterial != null)
        {
            if (turnOn)
                sphereMaterial.EnableKeyword("_EMISSION");
            else
                sphereMaterial.DisableKeyword("_EMISSION");
        }
    }
}