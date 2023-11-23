using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.HighDefinition;

public class FakeWind : MonoBehaviour
{
    [SerializeField]
    private bool organicWind;

    [Header("Light Data")]
    [SerializeField]
    private float pulseSpeed;
    [SerializeField]
    private float pulseStrenght;

    private Cloth cloth;

    private void Start()
    {
        cloth = GetComponent<Cloth>();
    }
    private void Update()
    {
        if (organicWind)
        {
            float newExternalVelocity = (Mathf.PerlinNoise1D((Time.time) * pulseSpeed) - .5f) * pulseStrenght;
            cloth.externalAcceleration = (cloth.externalAcceleration + ((-transform.right) * newExternalVelocity)).magnitude * (-transform.right);
        }
    }
}
