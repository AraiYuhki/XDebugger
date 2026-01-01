using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Common
{
    public enum TriggerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum TriggerMode
    {
        DoubleTap,
        TripleTap,
        DoubleFingerHold,
        TripleFingerHold,
    }

    [CreateAssetMenu(fileName = "XDebuggerSetting", menuName = "XDebugger/Setting")]
    public class XDebuggerSetting : ScriptableObject
    {
        [Header("Trigger Settings")]
        [SerializeField]
        private TriggerPosition triggerPosition;
        [SerializeField]
        private TriggerMode triggerMode;

        [SerializeField]
        private float inputGraceTime;

        [SerializeField]
        private float holdTimeForShow;
        
        [SerializeField]
        private UIFactoryBase uiFactory;

        [Header("MainMenu Settings")]
        [SerializeField]
        private TabButton tabButtonPrefab;
        [SerializeField]
        private StaticPageControl[] topPageTabList;

        public TriggerPosition TriggerPosition => triggerPosition;

        public TriggerMode TriggerMode => triggerMode;

        public float InputGraceTime => inputGraceTime;

        public float HoldTimeForShow => holdTimeForShow;

        public UIFactoryBase UIFactory => uiFactory;

        public TabButton TabButtonPrefab => tabButtonPrefab;

        public StaticPageControl[] TopPageTabList => topPageTabList;
    }
}
