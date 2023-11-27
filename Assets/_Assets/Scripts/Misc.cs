using System.Collections;
using System.Collections.Generic;
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

    [Header("Portrait Reference")]
    [SerializeField]
    private Renderer portraitReference;
    [SerializeField]
    private Texture2D dayBC;
    [SerializeField]
    private Texture2D dayNormal;
    [SerializeField]
    private Texture2D dayMask;
    [SerializeField]
    private Texture2D moonBC;
    [SerializeField]
    private Texture2D moonNormal;
    [SerializeField]
    private Texture2D moonMask;
    [SerializeField]
    private Texture2D bloodmoonBC;
    [SerializeField]
    private Texture2D bloodmoonNormal;
    [SerializeField]
    private Texture2D bloodmoonMask;

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

    [Header("Easter Egg")]
    [SerializeField]
    private string appName;
    [SerializeField]
    private List<KeyCode> triggerSequence;
    private List<KeyCode> currentSequence = new List<KeyCode>();

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
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    UnityEngine.Debug.Log("Key pressed: " + keyCode);
                    currentSequence.Add(keyCode);
                    if (currentSequence.Count > triggerSequence.Count)
                        currentSequence.RemoveAt(0);
                    if(currentSequence.Count == triggerSequence.Count)
                        CheckInput();
                    // Do something with the specific keycode
                    break; // Break the loop after finding the pressed key
                }
            }
        }
    }

    private void CheckInput()
    {
        if (currentSequence.Count == 0)
            return;
        for(int i = 0; i < currentSequence.Count; i++)
        {
            if (currentSequence[i] != triggerSequence[i])
                return;
        }
        UnityEngine.Debug.Log("Trigger Sequence");
        currentSequence.Clear();
        StartCastlevania();
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
        StartCoroutine(PortraitInterpolation(AmbianceType.BLOODMOON));
        currentType = AmbianceType.BLOODMOON;
    }

    public void RequestNormalMoon()
    {
        moon.GetComponent<HDAdditionalLightData>().angularDiameter = baseMoonSize;
        moon.GetComponent<HDAdditionalLightData>().surfaceTexture = moonTexture;
        StartCoroutine(LightInterpolation(baseMoonIntensity, baseMoonTemperature, baseMoonColor));
        StartCoroutine(VolumeInterpolation(AmbianceType.MOON));
        StartCoroutine(PortraitInterpolation(AmbianceType.MOON));
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
        StartCoroutine(PortraitInterpolation(AmbianceType.DAY));
        currentType = AmbianceType.DAY;
    }

    public void CycleTheme()
    {
        switch(currentType)
        {
            case AmbianceType.DAY:
                RequestNormalMoon();
                return;
            case AmbianceType.MOON:
                RequestBloodMoon();
                return;
            default:
                RequestSun();
                return;
        }
    }

    public IEnumerator LightInterpolation(float intensity, float temperature, Color color)
    {
        float baseIntensity = moon.GetComponent<HDAdditionalLightData>().intensity;
        float baseTemperature = moon.colorTemperature;
        Color baseColor = moon.GetComponent<HDAdditionalLightData>().surfaceTint;
        Light portraitLight = portraitReference.GetComponentInChildren<Light>();
        float portraitLightBaseTemp = portraitLight.colorTemperature;

        float t = 0f;
        while(t < interpolationTime)
        {
            moon.GetComponent<HDAdditionalLightData>().intensity = Mathf.Lerp(baseIntensity, intensity, t / interpolationTime);
            moon.colorTemperature = Mathf.Lerp(baseTemperature, temperature, t / interpolationTime);
            portraitLight.colorTemperature = Mathf.Lerp(portraitLightBaseTemp, temperature, t / interpolationTime);
            moon.GetComponent<HDAdditionalLightData>().surfaceTint = Color.Lerp(baseColor, color, t / interpolationTime);
            stainedGlass.GetComponent<HDAdditionalLightData>().intensity = moon.GetComponent<HDAdditionalLightData>().intensity * 10;

            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        moon.GetComponent<HDAdditionalLightData>().intensity = intensity;
        moon.colorTemperature = temperature;
        portraitLight.colorTemperature = Mathf.Lerp(portraitLightBaseTemp, temperature, t / interpolationTime);
        moon.GetComponent<HDAdditionalLightData>().surfaceTint = color;
        stainedGlass.GetComponent<HDAdditionalLightData>().intensity = intensity * 10;
        yield return null;
    }

    private IEnumerator PortraitInterpolation(AmbianceType newType)
    {
        Material mat = portraitReference.material;
        Texture2D newBC = dayBC;
        Texture2D newMask = dayMask;
        Texture2D newNormal = dayNormal;

        switch (newType)
        {
            case AmbianceType.MOON:
                newBC = moonBC;
                newMask = moonMask;
                newNormal = moonNormal;
                break;
            case AmbianceType.BLOODMOON:
                newBC = bloodmoonBC;
                newMask = bloodmoonMask;
                newNormal = bloodmoonNormal;
                break;
        }

        float t = 0;
        while (t < interpolationTime / 2f)
        {
            mat.SetFloat("_Fade", Mathf.Lerp(1f, 0f, t / (interpolationTime / 2f)));

            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        mat.SetFloat("_Fade", 0);

        mat.SetTexture("_MainTex", newBC);
        mat.SetTexture("_Normals", newNormal);
        mat.SetTexture("_Mask", newMask);

        t = 0;
        while (t < interpolationTime / 2f)
        {
            mat.SetFloat("_Fade", Mathf.Lerp(0f, 1f, t / (interpolationTime / 2f)));

            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        mat.SetFloat("_Fade", 1);
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

            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        currentVolume.weight = 0f;
        targetVolume.weight = 1f;
    }
}
