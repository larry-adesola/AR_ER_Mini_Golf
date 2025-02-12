using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARCameraManager))]
public class LightEstimationManager : MonoBehaviour
{
    ARCameraManager cameraManager;
    [SerializeField] Light sceneDirectionalLight;

    void Awake()
    {
        cameraManager = GetComponent<ARCameraManager>();
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            // Adjust intensity accordingly
            float brightness = args.lightEstimation.averageBrightness.Value;
            sceneDirectionalLight.intensity = brightness;
        }

        // if (args.lightEstimation.averageColorTemperature.HasValue)
        // {
        //     // Adjust color temperature
        //     float colorTemp = args.lightEstimation.averageColorTemperature.Value;
        //     sceneDirectionalLight.color = ColorUtility.ConvertKelvinToRGB(colorTemp);
        // }

        // If supported, you can also get directional light info:
        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            sceneDirectionalLight.transform.rotation =
                Quaternion.LookRotation(args.lightEstimation.mainLightDirection.Value);
        }

        if (args.lightEstimation.mainLightColor.HasValue)
        {
            sceneDirectionalLight.color = args.lightEstimation.mainLightColor.Value;
        }

        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            // For physically based units, adjust to your pipeline
            sceneDirectionalLight.intensity = args.lightEstimation.mainLightIntensityLumens.Value;
        }
    }
}
