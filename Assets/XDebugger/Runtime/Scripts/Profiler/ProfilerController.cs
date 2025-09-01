using Xeon.Common;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace Xeon.XDebugger.Profiler
{
    public class ProfilerController : MonoBehaviour
    {
        private struct FrameData
        {
            public double FrameTime;
            public double OtherTime;
            public double RenderTime;
            public double UpdateTime;
        }
        
        private const int FrameBufferSize = 400;
        
        private float averageFrameTime;
        private float lastFrameTime;
        private CircularBuffer<FrameData> frameBuffer = new(FrameBufferSize);

        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private readonly Stopwatch stopwatch = new();

        private Coroutine endOfFrameCoroutineHandler = null;

        [SerializeField]
        private LineGraph lineGraph;

        private void Awake()
        {
            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());
        }

        private void Update()
        {
            EndFrame();

            if (frameBuffer.Count > 0)
            {
                var frame = frameBuffer.Back();
                frame.FrameTime = Time.unscaledDeltaTime;
                frameBuffer[frameBuffer.Count - 1] = frame;
            }

            lastFrameTime = Time.unscaledDeltaTime;
            var frameCount = Mathf.Min(20, frameBuffer.Count);

            var f = 0d;
            for (var i = 0; i < frameCount; i++)
            {
                f += frameBuffer[frameBuffer.Count - 1 - i].FrameTime;
            }

            averageFrameTime = (float)f / frameCount;
            stopwatch.Start();
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
            frameBuffer.PushBack(new()
            {
                OtherTime = totalTime - updateTime - renderTime,
                UpdateTime = updateTime,
                RenderTime = renderTime
            });
            lineGraph.SetValues(frameBuffer.Select(data => (float)data.UpdateTime));
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
    }
}