using UnityEngine;
using UnityEngine.SceneManagement;

namespace Xeon.XDebugger.Control
{
    public class SystemInfoPage : StaticPageControl
    {
        [Header("System")]
        [SerializeField]
        private LabelControl osLabel;
        [SerializeField]
        private LabelControl deviceNameLabel;
        [SerializeField]
        private LabelControl deviceTypeLabel;
        [SerializeField]
        private LabelControl deviceModelLabel;
        [SerializeField]
        private LabelControl cpuTypeLabel;
        [SerializeField]
        private LabelControl cpuCountLabel;
        [SerializeField]
        private LabelControl systemMemoryLabel;

        [Header("Battery")]
        [SerializeField]
        private LabelControl batteryStatusLabel;
        [SerializeField]
        private LabelControl batteryLevelLabel;

        [Header("Unity")]
        [SerializeField]
        private LabelControl unityVersionLabel;
        [SerializeField]
        private LabelControl debugBuildLabel;
        [SerializeField]
        private LabelControl unityProLabel;
        [SerializeField]
        private LabelControl genuineLabel;
        [SerializeField]
        private LabelControl systemLanguageLabel;
        [SerializeField]
        private LabelControl platformLabel;
        [SerializeField]
        private LabelControl installModeLabel;
        [SerializeField]
        private LabelControl sandboxLabel;
        [SerializeField]
        private LabelControl il2cppLabel;
        [SerializeField]
        private LabelControl appVersionLabel;
        [SerializeField]
        private LabelControl appIdLabel;

        [Header("Display")]
        [SerializeField]
        private LabelControl resolutionLabel;
        [SerializeField]
        private LabelControl dpiLabel;
        [SerializeField]
        private LabelControl fullscreenLabel;
        [SerializeField]
        private LabelControl fullscreenModeLabel;
        [SerializeField]
        private LabelControl orientationLabel;

        [Header("Runtime")]
        [SerializeField]
        private LabelControl playTimeLabel;
        [SerializeField]
        private LabelControl levelPlaytimeLabel;
        [SerializeField]
        private LabelControl currentLevelLabel;
        [SerializeField]
        private LabelControl qualityLabel;

        [Header("Features")]
        [SerializeField]
        private LabelControl locationLabel;
        [SerializeField]
        private LabelControl accelerometerLabel;
        [SerializeField]
        private LabelControl gyroscopeLabel;
        [SerializeField]
        private LabelControl vibrationLabel;
        [SerializeField]
        private LabelControl audioLabel;

        [Header("Graphics - Device")]
        [SerializeField]
        private LabelControl graphicsDeviceNameLabel;
        [SerializeField]
        private LabelControl graphicsVendorLabel;
        [SerializeField]
        private LabelControl graphicsVersionLabel;
        [SerializeField]
        private LabelControl graphicsMemoryLabel;
        [SerializeField]
        private LabelControl maxTextureSizeLabel;

        [Header("Graphics - Features")]
        [SerializeField]
        private LabelControl uvStartsAtTopLabel;
        [SerializeField]
        private LabelControl shaderLevelLabel;
        [SerializeField]
        private LabelControl multiThreadedLabel;
        [SerializeField]
        private LabelControl hiddenSurfaceRemovalLabel;
        [SerializeField]
        private LabelControl uniformArrayIndexingLabel;
        [SerializeField]
        private LabelControl shadowLabel;
        [SerializeField]
        private LabelControl rawShadowDepthSamplingLabel;
        [SerializeField]
        private LabelControl motionVectorsLabel;
        [SerializeField]
        private LabelControl textures3dLabel;
        [SerializeField]
        private LabelControl arrayTextures2dLabel;
        [SerializeField]
        private LabelControl renderTextures3dLabel;
        [SerializeField]
        private LabelControl cubemapArrayTexturesLabel;
        [SerializeField]
        private LabelControl copyTextureSupportLabel;
        [SerializeField]
        private LabelControl computeShadersLabel;
        [SerializeField]
        private LabelControl instancingLabel;
        [SerializeField]
        private LabelControl hardwareQuadTopologyLabel;
        [SerializeField]
        private LabelControl index32bitLabel;
        [SerializeField]
        private LabelControl sparseTexturesLabel;
        [SerializeField]
        private LabelControl multisampledTexturesLabel;
        [SerializeField]
        private LabelControl textureWrapMirrorOnceLabel;
        [SerializeField]
        private LabelControl reversedZBufferLabel;

        [Header("iOS")]
        [SerializeField]
        private GameObject iosInfoGroup;
        [SerializeField]
        private LabelControl deviceGenerationLabel;
        [SerializeField]
        private LabelControl adTrackingLabel;

        private void OnEnable()
        {
            InitializeStaticLabels();
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateDynamicLabels();
            }
        }

        private void InitializeStaticLabels()
        {
            // iOS information group visibility
            UpdateiOSGroupVisibility();

            // System
            osLabel?.Setup($"OS : {SystemInfo.operatingSystem}");
            deviceNameLabel?.Setup($"Device : {SystemInfo.deviceName}");
            deviceTypeLabel?.Setup($"Type : {SystemInfo.deviceType}");
            deviceModelLabel?.Setup($"Model : {SystemInfo.deviceModel}");
            cpuTypeLabel?.Setup($"CPU : {SystemInfo.processorType}");
            cpuCountLabel?.Setup($"Core Count : {SystemInfo.processorCount}");
            systemMemoryLabel?.Setup($"System RAM : {GetSystemMemoryLabel()}");

            // Battery
            batteryStatusLabel?.Setup($"Battery Status : {SystemInfo.batteryStatus}");
            batteryLevelLabel?.Setup($"Battery Level : {SystemInfo.batteryLevel}");

            // Unity
            unityVersionLabel?.Setup($"Unity : {Application.unityVersion}");
            debugBuildLabel?.Setup($"Debug : {Debug.isDebugBuild}");
            unityProLabel?.Setup($"Pro : {Application.HasProLicense()}");
            var genuine = Application.genuine ? "Yes" : "No";
            var genuineCheck = Application.genuineCheckAvailable ? "Trusted" : "Untrusted";
            genuineLabel?.Setup($"Genuine : {genuine} ({genuineCheck})");
            systemLanguageLabel?.Setup($"Language : {Application.systemLanguage}");
            platformLabel?.Setup($"Platform : {Application.platform}");
            installModeLabel?.Setup($"Install Mode : {Application.installMode}");
            sandboxLabel?.Setup($"Sandbox : {Application.sandboxType}");
#if ENABLE_IL2CPP
            il2cppLabel?.Setup($"IL2CPP : Yes");
#else
            il2cppLabel?.Setup($"IL2CPP : No");
#endif
            appVersionLabel?.Setup($"App Version : {Application.version}");
            appIdLabel?.Setup($"App ID : {Application.identifier}");

            // Display
            resolutionLabel?.Setup($"Resolution : {Screen.width}x{Screen.height}");
            dpiLabel?.Setup($"DPI : {Screen.dpi}");
            fullscreenLabel?.Setup($"Fullscreen : {Screen.fullScreen}");
            fullscreenModeLabel?.Setup($"Fullscreen Mode : {Screen.fullScreenMode}");
            orientationLabel?.Setup($"Orientation : {Screen.orientation}");

            // Features
            locationLabel?.Setup($"Location : {SystemInfo.supportsLocationService}");
            accelerometerLabel?.Setup($"Accelerometer : {SystemInfo.supportsAccelerometer}");
            gyroscopeLabel?.Setup($"Gyroscope : {SystemInfo.supportsGyroscope}");
            vibrationLabel?.Setup($"Vibration : {SystemInfo.supportsVibration}");
            audioLabel?.Setup($"Audio : {SystemInfo.supportsAudio}");

            // iOS Information
            if (IsIOSPlatform())
            {
                deviceGenerationLabel?.Setup($"Device : {UnityEngine.iOS.Device.generation}");
                adTrackingLabel?.Setup($"Ad Tracking : {UnityEngine.iOS.Device.advertisingTrackingEnabled}");
            }

#pragma warning disable 618
            // Graphics - Device
            graphicsDeviceNameLabel?.Setup($"Graphics Device : {SystemInfo.graphicsDeviceName}");
            graphicsVendorLabel?.Setup($"GPU Vendor : {SystemInfo.graphicsDeviceVendor}");
            graphicsVersionLabel?.Setup($"GPU Version : {SystemInfo.graphicsDeviceVersion}");
            graphicsMemoryLabel?.Setup($"VRAM : {GetGraphicsMemoryLabel()}");
            maxTextureSizeLabel?.Setup($"Max Tex Size : {SystemInfo.maxTextureSize}");

            // Graphics - Features
            uvStartsAtTopLabel?.Setup($"UV Top : {SystemInfo.graphicsUVStartsAtTop}");
            shaderLevelLabel?.Setup($"Shader Level : {SystemInfo.graphicsShaderLevel}");
            multiThreadedLabel?.Setup($"Multi-threaded : {SystemInfo.graphicsMultiThreaded}");
            hiddenSurfaceRemovalLabel?.Setup($"Hidden Surface : {SystemInfo.hasHiddenSurfaceRemovalOnGPU}");
            uniformArrayIndexingLabel?.Setup($"Uniform Arrays : {SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders}");
            shadowLabel?.Setup($"Shadows : {SystemInfo.supportsShadows}");
            rawShadowDepthSamplingLabel?.Setup($"Raw Shadow Depth : {SystemInfo.supportsRawShadowDepthSampling}");
            motionVectorsLabel?.Setup($"Motion Vectors : {SystemInfo.supportsMotionVectors}");
            textures3dLabel?.Setup($"3D Textures : {SystemInfo.supports3DTextures}");
            arrayTextures2dLabel?.Setup($"2D Array Tex : {SystemInfo.supports2DArrayTextures}");
            renderTextures3dLabel?.Setup($"3D Render Tex : {SystemInfo.supports3DRenderTextures}");
            cubemapArrayTexturesLabel?.Setup($"Cubemap Array : {SystemInfo.supportsCubemapArrayTextures}");
            copyTextureSupportLabel?.Setup($"Copy Texture : {SystemInfo.copyTextureSupport}");
            computeShadersLabel?.Setup($"Compute Shaders : {SystemInfo.supportsComputeShaders}");
            instancingLabel?.Setup($"Instancing : {SystemInfo.supportsInstancing}");
            hardwareQuadTopologyLabel?.Setup($"Hardware Quad : {SystemInfo.supportsHardwareQuadTopology}");
            index32bitLabel?.Setup($"32-bit Index : {SystemInfo.supports32bitsIndexBuffer}");
            sparseTexturesLabel?.Setup($"Sparse Tex : {SystemInfo.supportsSeparatedRenderTargetsBlend}");
            multisampledTexturesLabel?.Setup($"MSAA Textures : {SystemInfo.supportsMultisampledTextures}");
            textureWrapMirrorOnceLabel?.Setup($"Wrap Mirror : {SystemInfo.supportsTextureWrapMirrorOnce}");
            reversedZBufferLabel?.Setup($"Reversed Z : {SystemInfo.usesReversedZBuffer}");
#pragma warning restore 618
        }

        private void UpdateDynamicLabels()
        {
            // Runtime - These change every frame
            playTimeLabel?.Setup($"Play Time : {Time.unscaledTime}");
            levelPlaytimeLabel?.Setup($"Level Play time : {Time.timeSinceLevelLoad}");
            
            var activeScene = SceneManager.GetActiveScene();
            currentLevelLabel?.Setup($"Current Level {activeScene.name} (Index: {activeScene.buildIndex})");
            
            var qualityLevel = QualitySettings.GetQualityLevel();
            qualityLabel?.Setup($"Quality Level : {QualitySettings.names[qualityLevel]} ({qualityLevel})");
        }

        private void UpdateiOSGroupVisibility()
        {
            if (iosInfoGroup != null)
            {
                iosInfoGroup.SetActive(IsIOSPlatform());
            }
        }

        private bool IsIOSPlatform()
        {
            return Application.platform == RuntimePlatform.IPhonePlayer;
        }

        private string GetSystemMemoryLabel()
        {
            decimal memorySize = (decimal)SystemInfo.systemMemorySize * 1024 * 1024;
            return GetLabelWithSISuffix(memorySize);
        }

        private string GetGraphicsMemoryLabel()
        {
            decimal memorySize = (decimal)SystemInfo.graphicsMemorySize * 1024 * 1024;
            return GetLabelWithSISuffix(memorySize);
        }

        private string GetLabelWithSISuffix(decimal value)
        {
            string[] suffix = new[] { "B", "KB", "MB", "GB", "TB" };
            var index = 0;
            for (; index < suffix.Length; index++)
            {
                if (value < 1024)
                {
                    break;
                }
                value /= 1024;
            }
            return $"{value:0.##}{suffix[index]}";
        }
    }
}
