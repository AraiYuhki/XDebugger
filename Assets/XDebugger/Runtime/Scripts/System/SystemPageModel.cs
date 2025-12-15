using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public class SystemPageModel : PageModel
    {
        private LabelModel playTime;
        private LabelModel levelPlaytime;
        private LabelModel currentLevel;
        private LabelModel qualityLabel;

        public override void Initialize(IUIFactory uiFactory)
        {
            this.uiFactory = uiFactory;
            using (HorizontalScope(string.Empty))
            {
                using (VerticalScope("System"))
                {
                    AddLabel($"Operating System : {SystemInfo.operatingSystem}");
                    AddLabel($"Device Name : {SystemInfo.deviceName}");
                    AddLabel($"Device Type : {SystemInfo.deviceType}");
                    AddLabel($"Device Model : {SystemInfo.deviceModel}");
                    AddLabel($"CPU Type : {SystemInfo.processorType}");
                    AddLabel($"CPU Count : {SystemInfo.processorCount}");
                    AddLabel($"System Memoty : {GetSystemMemoryLabel()}");
                    using (VerticalScope("Battery"))
                    {
                        AddLabel($"Status : {SystemInfo.batteryStatus}");
                        AddLabel($"Battery Level : {SystemInfo.batteryLevel}");
                    }
                }
                using (VerticalScope("Unity"))
                {
                    AddLabel($"Version : {Application.unityVersion}");
                    AddLabel($"Debug : {Debug.isDebugBuild}");
                    AddLabel($"Unity Pro : {Application.HasProLicense()}");
                    var genuie = Application.genuine ? "Yes" : "No";
                    var genuineCheck = Application.genuineCheckAvailable ? "Trusted" : "Untrusted";
                    AddLabel($"Genuine : {genuie}, {genuineCheck}");
                    AddLabel($"Sytem Language : {Application.systemLanguage}");
                    AddLabel($"Platform : {Application.platform}");
                    AddLabel($"Install Mode : {Application.installMode}");
                    AddLabel($"Sandbox : {Application.sandboxType}");
#if ENABLE_IL2CPP
                    const string IL2CPP = "Yes";
#else
                    const string IL2CPP = "No";
#endif
                    AddLabel($"IL2CPP : {IL2CPP}");
                    AddLabel($"Application Version : {Application.version}");
                    AddLabel($"Application ID : {Application.identifier}");
                }
            }
            using (HorizontalScope(""))
            {
                using (VerticalScope("Display"))
                {
                    AddLabel($"Resolution : {Screen.width} x {Screen.height}");
                    AddLabel($"DPI : {Screen.dpi}");
                    AddLabel($"Fullscreen : {Screen.fullScreen}");
                    AddLabel($"Fullscreen Mode : {Screen.fullScreenMode}");
                    AddLabel($"Orientation : {Screen.orientation}");
                }
                using (VerticalScope("Runtime"))
                {
                    playTime = AddLabel($"Play Time : {Time.unscaledTime}");
                    levelPlaytime = AddLabel($"Level Play time : {Time.timeSinceLevelLoad}");
                    var activeScene = SceneManager.GetActiveScene();
                    var label = $"Current Level {activeScene.name} (Index: {activeScene.buildIndex})";
                    currentLevel = AddLabel(label);
                    qualityLabel = AddLabel($"Quality Level : {QualitySettings.names[QualitySettings.GetQualityLevel()]} ({QualitySettings.GetQualityLevel()})");
                }
            }

            var cloudBuildManifest = (TextAsset)Resources.Load("UnityCloudBuildManifest.json");
            var manifestDict = cloudBuildManifest != null
                ? JsonUtility.FromJson<Dictionary<string, object>>(cloudBuildManifest.text)
                : null;
            if (manifestDict != null)
            {
                using (VerticalScope("Build"))
                {
                    foreach (var (key, value) in manifestDict)
                    {
                        if (value == null) continue;
                        AddLabel($"{GetCloundManifestPrettyName(key)}, {value}");
                    }
                }
            }
            using (VerticalScope("Features"))
            {
                AddLabel($"Location : {SystemInfo.supportsLocationService}");
                AddLabel($"Accelerometer : {SystemInfo.supportsAccelerometer}");
                AddLabel($"Gyroscope : {SystemInfo.supportsGyroscope}");
                AddLabel($"Vibration : {SystemInfo.supportsVibration}");
                AddLabel($"Audio : {SystemInfo.supportsAudio}");
            }
#if UNITY_IOS
            using (VerticalScope("iOS"))
            {
                AddLabel($"Generation : {UnityEngine.iOS.Device.generation}");
                AddLabel($"Ad Tracking : {UnityEngine.iOS.Device.advertisingTrackingEnabled}");
            }
#endif
#pragma warning disable 618
            using (HorizontalScope(""))
            {
                using (VerticalScope("Graphic - Device"))
                {
                    AddLabel($"Device Name : {SystemInfo.graphicsDeviceName}");
                    AddLabel($"Device Vendor : {SystemInfo.graphicsDeviceVendor}");
                    AddLabel($"Device Version : {SystemInfo.graphicsDeviceVersion}");
                    AddLabel($"Graphics Memory : {GetGraphicsMemoryLabel()}");
                    AddLabel($"Max Tex Size : {SystemInfo.maxTextureSize}");
                }
                using (VerticalScope("Graphics - Features"))
                {
                    AddLabel($"UV Starts at top : {SystemInfo.graphicsUVStartsAtTop}");
                    AddLabel($"Shader Level : {SystemInfo.graphicsShaderLevel}");
                    AddLabel($"Multi threaded : {SystemInfo.graphicsMultiThreaded}");
                    AddLabel($"Hidden Service Removal (GPU) : {SystemInfo.hasHiddenSurfaceRemovalOnGPU}");
                    AddLabel($"Uniform Array Indexing (Fragment Shaders) : {SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders}");
                    AddLabel($"Shadow : {SystemInfo.supportsShadows}");
                    AddLabel($"Raw Depth Sampling (Shadows) : {SystemInfo.supportsRawShadowDepthSampling}");
                    AddLabel($"Motion Vectors : {SystemInfo.supportsMotionVectors}");
                    AddLabel($"3D Textures : {SystemInfo.supports3DTextures}");
                    AddLabel($"2D Array Textures : {SystemInfo.supports2DArrayTextures}");
                    AddLabel($"3D Render Textures : {SystemInfo.supports3DRenderTextures}");
                    AddLabel($"Cubemap Array Textures: {SystemInfo.supportsCubemapArrayTextures}");
                    AddLabel($"Copy Texture Support : {SystemInfo.copyTextureSupport}");
                    AddLabel($"Compute Shaders : {SystemInfo.supportsComputeShaders}");
                    AddLabel($"Instancing : {SystemInfo.supportsInstancing}");
                    AddLabel($"Hadware Quad Topology : {SystemInfo.supportsHardwareQuadTopology}");
                    AddLabel($"32-bitt index buffer : {SystemInfo.supports32bitsIndexBuffer}");
                    AddLabel($"Sparse Textures : {SystemInfo.supportsSeparatedRenderTargetsBlend}");
                    AddLabel($"Multisampled Textures : {SystemInfo.supportsMultisampledTextures}");
                    AddLabel($"Texture Wrap Mirror One : {SystemInfo.supportsTextureWrapMirrorOnce}");
                    AddLabel($"Reversed Z Buffer : {SystemInfo.usesReversedZBuffer}");
                }
            }
#pragma warning restore 618
        }

        public override void Update()
        {
            playTime.SetText($"Play Time : {Time.unscaledTime}");
            levelPlaytime.SetText($"Level Play time : {Time.timeSinceLevelLoad}");
            var activeScene = SceneManager.GetActiveScene();
            var label = $"Current Level {activeScene.name} (Index: {activeScene.buildIndex})";
            currentLevel.SetText(label);
            qualityLabel.SetText($"Quality Level : {QualitySettings.names[QualitySettings.GetQualityLevel()]} ({QualitySettings.GetQualityLevel()})");
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

        private static string GetCloundManifestPrettyName(string name)
        {
            return name switch
            {
                "scmCommitId" => "Commit",
                "scmBranch" => "Branch",
                "cloudBuildTargetName" => "Build Target",
                "buildStartTime" => "Build Date",
                _ => name.Substring(0, 1).ToUpper() + name.Substring(1)
            };
        }
    }
}
