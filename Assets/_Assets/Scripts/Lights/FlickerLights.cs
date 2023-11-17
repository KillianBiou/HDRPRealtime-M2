using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(HDAdditionalLightData))]
public class FlickerLightsOrganic : MonoBehaviour
{
    [SerializeField]
    private bool organicFlicker;

    [Header("Light Data")]
    [SerializeField]
    private float flickerSpeed;
    [SerializeField]
    private float flickerStrenght;

    private float baseIntensity;
    private float offset;
    private HDAdditionalLightData lightData;

    private void Start()
    {
        lightData = GetComponent<HDAdditionalLightData>();
        baseIntensity = lightData.intensity;
        offset = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        if(organicFlicker)
        {
            float newIntensity = baseIntensity + (Mathf.PerlinNoise1D((Time.time + offset) * flickerSpeed) - .5f) * flickerStrenght;
            lightData.intensity = newIntensity;
        }
    }
}
