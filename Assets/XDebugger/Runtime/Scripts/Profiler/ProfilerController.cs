using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Xeon.Common;

namespace Xeon.XDebugger.Profiler
{
    public class ProfilerController : MonoBehaviour
    {
        private const int FrameBufferSize = 100;

        private struct FrameData : IStackedBarItemData
        {
            public float UpdateTime;
            public float RenderTime;
            public float OtherTime;

            public int Count => 3;
            public float this[int index]
            {
                get
                {
                    return index switch
                    {
                        0 => UpdateTime,
                        1 => RenderTime,
                        2 => OtherTime,
                        _ => throw new IndexOutOfRangeException(),
                    };
                }
            }

            public FrameData(double totalTime, double updateTime, double renderTime)
            {
                UpdateTime = (float)totalTime;
                RenderTime = UpdateTime - (float)updateTime;
                OtherTime = RenderTime - (float)renderTime;

            }

            public IEnumerator<float> GetEnumerator()
            {
                yield return UpdateTime;
                yield return RenderTime;
                yield return OtherTime;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [SerializeField]
        private StackedBarChart graph;

        private float fps = 0f;
        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private float averageFrameTime;
        private readonly Stopwatch stopwatch = new();

        private Coroutine endOfFrameCoroutineHandler = null;

        private DoubleCircularBuffer totalTimeBuffer = new(FrameBufferSize);
        private CircularBuffer<float> frameBuffer = new(FrameBufferSize);

        private void Awake()
        {
            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());

            var buffer = new CircularBuffer<IStackedBarItemData>(FrameBufferSize, Enumerable.Repeat<IStackedBarItemData>(new FrameData(0, 0, 0), FrameBufferSize).ToArray());
            graph.Initialize(buffer, new Color[] {Color.cyan, Color.green, Color.magenta }, true);
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
            totalTimeBuffer.PushBack(totalTime);
            graph.SetMax((float)totalTimeBuffer.Max * 1.2f);
            graph.AddValue(new FrameData(totalTime, updateTime, renderTime));
            
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