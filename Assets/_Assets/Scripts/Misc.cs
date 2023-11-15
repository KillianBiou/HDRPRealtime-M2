using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Misc : MonoBehaviour
{
    [Header("Global Ambiance")]
    [SerializeField]
    private float baseMoonIntensity;
    [SerializeField]
    private Color baseMoonColor;
    [SerializeField]
    private float baseMoonTemperature;
    [SerializeField]
    private float baseMoonSize;
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

    [Header("Reference")]
    [SerializeField]
    private Light moon;
    [SerializeField]
    private Texture moonTexture;
    [SerializeField]
    private Texture sunTexture;

    [SerializeField]
    private string appName;

    public static Misc instance;

    private bool requestBloodmoon;

    private void Start()
    {
        instance = this;
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
        moon.colorTemperature = bloodMoonTemperature;
        moon.intensity = bloodMoonIntensity;
        moon.GetComponent<HDAdditionalLightData>().surfaceTint = bloodMoonColor;
        moon.GetComponent<HDAdditionalLightData>().angularDiameter = bloodMoonSize;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(true));
    }

    public void RequestNormalMoon()
    {
        UnityEngine.Debug.Log("Normal Moon");
        moon.colorTemperature = baseMoonTemperature;
        moon.intensity = baseMoonIntensity;
        moon.GetComponent<HDAdditionalLightData>().surfaceTint = baseMoonColor;
        moon.GetComponent<HDAdditionalLightData>().angularDiameter = baseMoonSize;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(true));
    }

    public void RequestSun()
    {
        UnityEngine.Debug.Log("Sun");
        moon.colorTemperature = baseSunTemperature;
        moon.intensity = baseSunIntensity;
        moon.GetComponent<HDAdditionalLightData>().surfaceTint = baseSunColor;
        moon.GetComponent<HDAdditionalLightData>().angularDiameter = baseSunSize;
        requestBloodmoon = false;
        StartCoroutine(VolumeInterpolation(false));
    }

    public IEnumerator VolumeInterpolation(bool toNight)
    {
        float t = 0;
        while(t < interpolationTime)
        {
            if(toNight)
            {
                if (moonVolume.weight == 1)
                    break;
                moonVolume.weight = Mathf.Lerp(0f, 1f, t /  interpolationTime);
                sunVolume.weight = Mathf.Lerp(1f, 0f, t /  interpolationTime);
            }
            else
            {
                if (sunVolume.weight == 1)
                    break;
                moonVolume.weight = Mathf.Lerp(1f, 0f, t / interpolationTime);
                sunVolume.weight = Mathf.Lerp(0f, 0f, t / interpolationTime);
            }

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
        if (toNight)
        {
            moonVolume.weight = 1;
            sunVolume.weight = 0;
        }
        else
        {
            moonVolume.weight = 0;
            sunVolume.weight = 1;
        }
        yield return null;
    }
}
