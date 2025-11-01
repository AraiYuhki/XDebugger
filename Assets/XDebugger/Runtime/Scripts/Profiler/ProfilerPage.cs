using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Xeon.Common;
using Xeon.XDebugger.Control;
using Xeon.XGraph.Model;
using Xeon.XGraph.View;

namespace Xeon.XDebugger.Profiler
{
    public class ProfilerPage : PageControl
    {
        private const int FrameBufferSize = 400;
        [FormerlySerializedAs("newGraph")] [SerializeField]
        private StackedBarChart graph;
        [SerializeField]
        private TMP_Text totalAllocatedMemoryText;
        [SerializeField]
        private TMP_Text usedMemoryText;
        [SerializeField]
        private Slider memoryGauge;
        [SerializeField]
        private TMP_Text totalAllocatedMonoText;
        [SerializeField]
        private TMP_Text usedMonoText;
        [SerializeField]
        private Slider monoGauge;
        [SerializeField]
        private TMP_Text fpsLabel;

        private static readonly string[] suffixList = { "B", "KB", "MB", "GB", "TB" };

        public bool IsMonoSupported { get; private set; } = false;

        private float fps = 0f;
        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private float lastRefreshedTime = 0f;

        private readonly Stopwatch stopwatch = new();

        private Coroutine endOfFrameCoroutineHandler = null;

        private MultiValueSeries timeBuffer;

        private DoubleCircularBuffer totalTimeBuffer = new(FrameBufferSize);

        private void Awake()
        {
            timeBuffer = new MultiValueSeries(new List<Series>()
            {
                new Series("UpdateTime", Color.darkSeaGreen, new CircularBuffer<float>(FrameBufferSize, Enumerable.Repeat(0f, FrameBufferSize).ToArray())),
                new Series("RenderTime", Color.cadetBlue, new CircularBuffer<float>(FrameBufferSize, Enumerable.Repeat(0f, FrameBufferSize).ToArray())),
                new Series("OtherTime", Color.burlywood, new CircularBuffer<float>(FrameBufferSize, Enumerable.Repeat(0f, FrameBufferSize).ToArray()))
            }, Color.white);
            
            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());
            
            graph.Initialize(timeBuffer);
            IsMonoSupported = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() > 0;

            memoryGauge.minValue = 0f;
            monoGauge.minValue = 0f;
            
            graph.MarkerManager.SetMarkers(new List<BarGraphMarkerData>() { new ("15FPS", 0.0667f), new ("30FPS", 0.0333f), new ("60FPS", 0.0167f), new ("90FPS", 0.0111f), new ("120FPS", 0.0083f) });
        }

        private void Update()
        {
            EndFrame();

            fps = 1f / Time.deltaTime;
            fpsLabel.text = $"FPS: {Mathf.RoundToInt(fps)}";

            stopwatch.Start();

            if (Time.realtimeSinceStartup - lastRefreshedTime > 1f)
            {
                RefreshMemoryInfo();
                RefreshMonoInfo();
                lastRefreshedTime = Time.realtimeSinceStartup;
            }
        }

        private IEnumerator EndOfFrameCoroutine()
        {
            var endOfFrame = new WaitForEndOfFrame();
            while (true)
            {
                yield return endOfFrame;
                renderDuration = stopwatch.Elapsed.TotalSeconds - renderStartTime;
            }
        }

        private void LateUpdate()
        {
            updateDuration = stopwatch.Elapsed.TotalSeconds;
        }

        private void PushFrameData(double totalTime, double updateTime, double renderTime)
        {
            totalTimeBuffer.PushBack(totalTime);
            
            graph.SetMax((float)totalTimeBuffer.Max * 1.1f);
            timeBuffer.AddValue((float)updateTime, (float)renderTime, (float)(totalTime - updateTime - renderTime));
        }

        private void EndFrame()
        {
            if (stopwatch.IsRunning)
            {
                PushFrameData(stopwatch.Elapsed.TotalSeconds, updateDuration, renderDuration);
                stopwatch.Reset();
                stopwatch.Start();
            }

            updateDuration = renderDuration = 0;
        }

        private void RenderPipelineOnBeginFrameRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            renderStartTime = stopwatch.Elapsed.TotalSeconds;
        }

        private void RefreshMemoryInfo()
        {
            var max = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            var current = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();

            memoryGauge.maxValue = max;
            memoryGauge.value = current;
            totalAllocatedMemoryText.text = $"Reserved: {GetValueWithSISuffix(max)}";
            usedMemoryText.text = GetValueWithSISuffix(current);
        }

        private void RefreshMonoInfo()
        {
            var max = IsMonoSupported ? UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() : GC.GetTotalMemory(false);
            var current = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();

            monoGauge.maxValue = max;
            monoGauge.value = current;
            totalAllocatedMonoText.text = GetValueWithSISuffix((double)max);
            usedMonoText.text = GetValueWithSISuffix((double)current);
        }

        private string GetValueWithSISuffix(double value)
        {
            var index = 0;
            while (value > 1024)
            {
                value /= 1024d;
                index++;
            }
            value = Math.Round(value, 2);
            return $"{value}{suffixList[index]}";
        }

        public void TriggerCleanupMemory()
        {
            StartCoroutine(CleanupMemory());
        }

        public void GCCollect()
        {
            GC.Collect();
            RefreshMonoInfo();
        }

        private IEnumerator CleanupMemory()
        {
            GC.Collect();
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
            RefreshMemoryInfo();
        }
    }
}