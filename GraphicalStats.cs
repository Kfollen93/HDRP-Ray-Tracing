using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Color = UnityEngine.Color;

public class GraphicalStats : MonoBehaviour
{
    [Header("Camera Data")]
    [SerializeField] private Camera _mainCam;
    private HDAdditionalCameraData _cameraData;
    private FrameSettings _frameSettings;
    private FrameSettingsOverrideMask _frameSettingsOverrideMask;

    [Header("Ray Tracing")]
    [SerializeField] private VolumeProfile _volumeProfile;
    [SerializeField] private TMP_Text _rtGlobalText;
    [SerializeField] private TMP_Text _rtLightsText;
    [SerializeField] private TMP_Text _rtReflectionsText;
    [SerializeField] private TMP_Text _rtGlobalIlluminationText;
    [SerializeField] private TMP_Text _rtAmbientOcclusionText;
    private HDAdditionalLightData[] _lightData;
    private TMP_Text[] _textFields;
    private ScreenSpaceReflection _ssr;
    private GlobalIllumination _gi;
    private ScreenSpaceAmbientOcclusion _ssao;
    private bool _globalRT = true, _shadowsRT = true, _reflectionsRT = true, _globalIlluminationRT = true, _ambientOcclusionRT = true;

    /* I've added a quick DLSS toggle via keypress('T') as an extra option. */
    [Header("DLSS")]
    [SerializeField] private TMP_Text _dlssUI;
    private bool _dlssStatus = false;
    private int _dlssModes = 0;
    private readonly Dictionary<int, string> _dlssPairs = new() { { 0, "Max Performance" }, { 1, "Balanced" }, { 2, "Max Quality" } };

    void Start()
    {
        // Hold text fields for setting text value and color.
        _textFields = new TMP_Text[5] { _rtGlobalText, _rtLightsText, _rtReflectionsText, _rtGlobalIlluminationText, _rtAmbientOcclusionText };

        // Camera set up.
        _cameraData = _mainCam.GetComponent<HDAdditionalCameraData>();
        _frameSettings = _cameraData.renderingPathCustomFrameSettings;
        _frameSettingsOverrideMask = _cameraData.renderingPathCustomFrameSettingsOverrideMask;
        _cameraData.customRenderingSettings = true;

        // Enabling Ray Tracing bit & applying the frame setting mask back to the camera.
        _frameSettingsOverrideMask.mask[(uint)FrameSettingsField.RayTracing] = true;
        _cameraData.renderingPathCustomFrameSettingsOverrideMask = _frameSettingsOverrideMask;

        // Find all scene lights.
        _lightData = FindObjectsOfType<HDAdditionalLightData>();

        // Enable all Ray Traced effects on Start.
        GetPostProcessingComponents();
        SetAllRayTracingEffects(true, true, RayCastingMode.RayTracing, true, Color.green, "On");
    }

    void Update()
    {
        // Track & Update the Custom Frame Settings (Pertains to toggling the Ray Tracing setting on the camera).
        _cameraData.renderingPathCustomFrameSettings = _frameSettings;
        ToggleDLSS();
    }

    public void OnButtonClick_ToggleRTGlobal()
    {
        _globalRT = !_globalRT;
        if (_globalRT)
        {
            SetAllRayTracingEffects(true, true, RayCastingMode.RayTracing, true, Color.green, "On");
            SetAlLToggleBoolValues(true);
        }
        else
        {
            SetAllRayTracingEffects(false, false, RayCastingMode.RayMarching, false, Color.red, "Off");
            SetAlLToggleBoolValues(false);
        }
    }

    public void OnButtonClick_ToggleRTGlobalIllumination()
    {
        if (!_globalRT)
        {
            Debug.Log("Can't toggle Global Illumination, Global RT is off!");
            return;
        }
        _globalIlluminationRT = !_globalIlluminationRT;
        SetRayTracedGlobalIllumination(_globalIlluminationRT);
    }

    public void OnButtonClick_ToggleRTShadows()
    {
        if (!_globalRT)
        {
            Debug.Log("Can't toggle Shadows, Global RT is off!");
            return;
        }
        _shadowsRT = !_shadowsRT;
        SetRayTracedShadowsFromLights(_shadowsRT);
    }

    public void OnButtonClick_ToggleRTReflections()
    {
        if (!_globalRT)
        {
            Debug.Log("Can't toggle Reflections, Global RT is off!");
            return;
        } 
        _reflectionsRT = !_reflectionsRT;
        SetRayTracedReflections(_reflectionsRT);
    }

    public void OnButtonClick_ToggleRTAmbientOcclusion()
    {
        if (!_globalRT)
        {
            Debug.Log("Can't toggle Ambient Occlusion, Global RT is off!");
            return;
        }
        _ambientOcclusionRT = !_ambientOcclusionRT;
        SetRayTracedAmbientOcclusion(_ambientOcclusionRT);
    }

    private void ToggleDLSS()
    {
        _cameraData.allowDeepLearningSuperSampling = _dlssStatus;
        if (Input.GetKeyDown(KeyCode.T)) _dlssStatus = !_dlssStatus;

        if (!_dlssStatus)
        {
            _dlssUI.text = string.Empty;
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_dlssModes < 2) _dlssModes++;
            else _dlssModes = 0;
        } 
       
        _cameraData.deepLearningSuperSamplingQuality = (uint)_dlssModes;
        _dlssUI.text = $"DLSS Mode: {_dlssPairs[_dlssModes]}";
    }

    private void GetPostProcessingComponents()
    {
        if (_volumeProfile.TryGet<ScreenSpaceReflection>(out var ssr)) _ssr = ssr;
        else Debug.LogError("Screen Space Reflections is null. Ensure you have the added the override to the Volume profile");

        if (_volumeProfile.TryGet<GlobalIllumination>(out var gi)) _gi = gi;
        else Debug.LogError("Global Illumination is null. Ensure you have the added the override to the Volume profile");

        if (_volumeProfile.TryGet<ScreenSpaceAmbientOcclusion>(out var ssao)) _ssao = ssao;
        else Debug.LogError("Screen Space Ambient Occlusion is null. Ensure you have the added the override to the Volume profile");
    }

    private void SetAllRayTracingEffects(bool globalRT, bool rtShadowsFromLights, RayCastingMode rayCastMode, bool ssaoRT, Color color, string status)
    {
        SetGlobalRayTracingOnCameraFrameSettings(globalRT);
        SetRayTracedShadowsFromLights(rtShadowsFromLights);
        SetRayTracedPostProcessingSettings(rayCastMode, ssaoRT);
        SetAllTextFields(color, status);
    }

    private void SetGlobalRayTracingOnCameraFrameSettings(bool globalRayTracing)
    {
        _frameSettings.SetEnabled(FrameSettingsField.RayTracing, globalRayTracing);
        _globalRT = _frameSettings.IsEnabled(FrameSettingsField.RayTracing);
        _rtGlobalText.text = _globalRT ? "On" : "Off";
        _rtGlobalText.color = _globalRT ? Color.green : Color.red;
    }

    private void SetRayTracedShadowsFromLights(bool rayTracedShadows)
    {
        if (_lightData.Length == 0)
        {
            _rtLightsText.text = "Off";
            _rtLightsText.color = Color.red;
            Debug.Log("There are no lights in the scene.");
            return;
        }

        foreach (var light in _lightData)
        {
            light.useRayTracedShadows = rayTracedShadows;
        }

        _rtLightsText.text = rayTracedShadows ? "On" :"Off";
        _rtLightsText.color = rayTracedShadows ? Color.green : Color.red;
    }

    private void SetRayTracedPostProcessingSettings(RayCastingMode rayCastingMode, bool rtSSAO)
    {
        /* Enabling and disabling the settings in here apply to the Volume Profile, therefore they will remain in the state that you left them at when you exit play mode */
        _ssr.tracing.value = rayCastingMode;
        _gi.tracing.value = rayCastingMode;
        _ssao.rayTracing.value = rtSSAO;
    }

    private void SetRayTracedReflections(bool rtReflections)
    {
        _ssr.tracing.value = rtReflections ? RayCastingMode.RayTracing : RayCastingMode.RayMarching;
        _rtReflectionsText.text = rtReflections ? "On" : "Off";
        _rtReflectionsText.color = rtReflections ? Color.green : Color.red;
    }

    private void SetRayTracedGlobalIllumination(bool rtGlobalIllumination)
    {
        _gi.tracing.value = rtGlobalIllumination ? RayCastingMode.RayTracing : RayCastingMode.RayMarching;
        _rtGlobalIlluminationText.text = rtGlobalIllumination ? "On" : "Off";
        _rtGlobalIlluminationText.color = rtGlobalIllumination ? Color.green : Color.red;
    }

    private void SetRayTracedAmbientOcclusion(bool rtAmbientOcclusion)
    {
        _ssao.rayTracing.value = rtAmbientOcclusion;
        _rtAmbientOcclusionText.text = rtAmbientOcclusion ? "On" : "Off";
        _rtAmbientOcclusionText.color = rtAmbientOcclusion ? Color.green : Color.red;
    }

    private void SetAllTextFields(Color color, string status)
    {
        foreach (var ele in _textFields)
        {
            ele.color = color;
            ele.text = status;
        }
    }

    private void SetAlLToggleBoolValues(bool status)
    {
        _shadowsRT = status;
        _reflectionsRT = status;
        _globalIlluminationRT = status;
        _ambientOcclusionRT = status;
    }
}