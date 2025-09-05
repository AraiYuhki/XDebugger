using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Xeon.Common;

namespace Xeon.XDebugger.Profiler
{
    public class ProfilerController : MonoBehaviour
    {
        private const int FrameBufferSize = 400;

        [SerializeField]
        private LineGraph updateTimeGraph;
        [SerializeField]
        private LineGraph renderTimeGraph;
        [SerializeField]
        private LineGraph otherTimeGraph;

        private float fps = 0f;
        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private float averageFrameTime;
        private readonly Stopwatch stopwatch = new();

        private Coroutine endOfFrameCoroutineHandler = null;

        private CircularBuffer<float> frameBuffer = new(FrameBufferSize);

        private void Awake()
        {
            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());

            updateTimeGraph.Initialize(FrameBufferSize);
            renderTimeGraph.Initialize(FrameBufferSize);
            otherTimeGraph.Initialize(FrameBufferSize);
        }

        private void Update()
        {
            EndFrame();

            fps = 1f / Time.deltaTime;

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
            updateTimeGraph.AddValue(updateTime, false);
            renderTimeGraph.AddValue(renderTime, false);
            otherTimeGraph.AddValue(totalTime - updateTime - renderTime, false);

            var max = (float)Math.Max(updateTimeGraph.MaxValue, renderTimeGraph.MaxValue);
            max = (float)Math.Max(max, otherTimeGraph.MaxValue) * 1.2f;
            updateTimeGraph.SetMax(max);
            renderTimeGraph.SetMax(max);
            otherTimeGraph.SetMax(max);
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