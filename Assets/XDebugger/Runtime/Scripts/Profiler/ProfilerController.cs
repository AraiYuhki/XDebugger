using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Xeon.XDebugger.Profiler
{
    public class ProfilerController : MonoBehaviour
    {
        private const int FrameBufferSize = 400;

        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private readonly Stopwatch stopwatch = new();

        private Coroutine endOfFrameCoroutineHandler = null;

        [SerializeField]
        private LineGraph frameTimeGraph;
        [SerializeField]
        private LineGraph updateTimeGraph;
        [SerializeField]
        private LineGraph renderTimeGraph;
        [SerializeField]
        private LineGraph otherTimeGraph;

        private void Awake()
        {
            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());

            frameTimeGraph.Initialize(FrameBufferSize);
            updateTimeGraph.Initialize(FrameBufferSize);
            renderTimeGraph.Initialize(FrameBufferSize);
            otherTimeGraph.Initialize(FrameBufferSize);
        }

        private void Update()
        {
            EndFrame();

            frameTimeGraph.AddValue(Time.unscaledDeltaTime);

            var values = frameTimeGraph.GetValues();
            var frameCount = Mathf.Min(20, values.Count);

            var f = 0d;
            var count = values.Count - 1;
            for (var i = 0; i < frameCount; i++)
            {
                f += values[count - i];
            }

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
            updateTimeGraph.AddValue(updateTime);
            renderTimeGraph.AddValue(renderTime);
            otherTimeGraph.AddValue(totalTime - updateTime - renderTime);
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