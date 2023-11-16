using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public enum AmbianceType
{
    DAY,
    MOON,
    BLOODMOON
};

public class Misc : MonoBehaviour
{
    [Header("Base Moon Ambiance")]
    [SerializeField]
    private float baseMoonIntensity;
    [SerializeField]
    private Color baseMoonColor;
    [SerializeField]
    private float baseMoonTemperature;
    [SerializeField]
    private float baseMoonSize;

    [Header("Blood Moon Ambiance")]
    [SerializeField]
    private float bloodMoonIntensity;
    [SerializeField]
    private Color bloodMoonColor;
    [SerializeField]
    private float bloodMoonTemperature;
    [SerializeField]
    private float bloodMoonSize;

    [Header("Day Ambiance")]
    [SerializeField]
    private Color baseSunColor;
    [SerializeField]
    private float baseSunTemperature;
    [SerializeField]
    private float baseSunSize;
    [SerializeField]
    private float baseSunIntensity;

    [Header("Interpolation Parameters")]
    [SerializeField]
    private float interpolationTime;
    [SerializeField]
    private Volume sunVolume;
    [SerializeField]
    private Volume moonVolume;
    [SerializeField]
    private Volume bloodmoonVolume;

    [Header("Reference")]
    [SerializeField]
    private Light moon;
    [SerializeField]
    private Light stainedGlass;
    [SerializeField]
    private Texture2D moonTexture;
    [SerializeField]
    private Texture2D sunTexture;
    [SerializeField]
    private Volume postProcessStack;

    [SerializeField]
    private string appName;

    public static Misc instance;

    private bool requestBloodmoon;

    private AmbianceType currentType;

    private void Start()
    {
        instance = this;
        currentType = AmbianceType.DAY;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            StartCastlevania();

        if (Input.GetKeyDown(KeyCode.I))
            RequestSun();
        if (Input.GetKeyDown(KeyCode.O))
            RequestNormalMoon();
        if (Input.GetKeyDown(KeyCode.P))
            RequestBloodMoon();


        if (requestBloodmoon)
            RequestBloodMoon();
    }

    private void StartCastlevania()
    {
        ProcessStartInfo psi = new ProcessStartInfo(Application.dataPath + "/Misc/" + appName);
        psi.WorkingDirectory = System.IO.Path.GetDirectoryName(Application.dataPath + "/Misc/" + appName);

        Process externalProcess = new Process();
        externalProcess.StartInfo = psi;
        externalProcess.EnableRaisingEvents = true; // Enable raising events

        // Attach the event handler for the Exited event
        externalProcess.Exited += ProgramExited;

        // Start the external process
        externalProcess.Start();
    }

    public void ProgramExited(object sender, System.EventArgs e)
    {
        requestBloodmoon = true;
    }


    public void RequestBloodMoon()
    {
        UnityEngine.Debug.Log("Bloodmoon");

        StartCoroutine(LightInterpolation(bloodMoonIntensity, bloodMoonTemperature, bloodMoonColor));

        moon.GetComponent<HDAdditionalLightData>().angularDiameter = bloodMoonSize;
        moon.GetComponent<HDAdditionalLightData>().surfaceTexture = moonTexture;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(AmbianceType.BLOODMOON));
        currentType = AmbianceType.BLOODMOON;
    }

    public void RequestNormalMoon()
    {
        UnityEngine.Debug.Log("Normal Moon");

        StartCoroutine(LightInterpolation(baseMoonIntensity, baseMoonTemperature, baseMoonColor));

        moon.GetComponent<HDAdditionalLightData>().angularDiameter = baseMoonSize;
        moon.GetComponent<HDAdditionalLightData>().surfaceTexture = moonTexture;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(AmbianceType.MOON));
        currentType = AmbianceType.MOON;
    }

    public void RequestSun()
    {
        UnityEngine.Debug.Log("Sun");

        StartCoroutine(LightInterpolation(baseSunIntensity, baseSunTemperature, baseSunColor));

        moon.GetComponent<HDAdditionalLightData>().angularDiameter = baseSunSize;
        moon.GetComponent<HDAdditionalLightData>().surfaceTexture = sunTexture;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(AmbianceType.DAY));
        currentType = AmbianceType.DAY;
    }

    public IEnumerator LightInterpolation(float intensity, float temperature, Color color)
    {
        float baseIntensity = moon.GetComponent<HDAdditionalLightData>().intensity;
        float baseTemperature = moon.colorTemperature;
        Color baseColor = moon.GetComponent<HDAdditionalLightData>().surfaceTint;

        float t = 0;
        while(t < interpolationTime)
        {
            moon.GetComponent<HDAdditionalLightData>().intensity = Mathf.Lerp(baseIntensity, intensity, t / interpolationTime);
            moon.colorTemperature = Mathf.Lerp(baseTemperature, temperature, t / interpolationTime);
            moon.GetComponent<HDAdditionalLightData>().surfaceTint = Color.Lerp(baseColor, color, t / interpolationTime);
            stainedGlass.GetComponent<HDAdditionalLightData>().intensity = moon.GetComponent<HDAdditionalLightData>().intensity * 10;

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        moon.GetComponent<HDAdditionalLightData>().intensity = intensity;
        moon.colorTemperature = temperature;
        moon.GetComponent<HDAdditionalLightData>().surfaceTint = color;
        stainedGlass.GetComponent<HDAdditionalLightData>().intensity = intensity * 10;

        yield return null;
    }

    public IEnumerator VolumeInterpolation(AmbianceType newType)
    {
        Volume currentVolume = sunVolume;
        switch (currentType)
        {
            case AmbianceType.MOON:
                currentVolume = moonVolume;
                break;
            case AmbianceType.BLOODMOON:
                currentVolume = bloodmoonVolume;
                break;
        }
        Volume targetVolume = sunVolume;
        switch (newType)
        {
            case AmbianceType.MOON:
                targetVolume = moonVolume;
                break;
            case AmbianceType.BLOODMOON:
                targetVolume = bloodmoonVolume;
                break;
        }

        float t = 0;
        while(t < interpolationTime)
        {
            if (targetVolume.weight == 1)
                break;
            currentVolume.weight = Mathf.Lerp(1f, 0f, t / interpolationTime);
            targetVolume.weight = Mathf.Lerp(0f, 1f, t / interpolationTime);

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        currentVolume.weight = 0f;
        targetVolume.weight = 1f;
        yield return null;
    }
}
