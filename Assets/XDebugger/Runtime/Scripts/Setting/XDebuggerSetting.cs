using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
        [CustomEditor(typeof(XDebuggerSetting))]
        private class  XDebuggerSettingEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var setting = target as XDebuggerSetting;
                EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
                setting.triggerMode = (TriggerMode)EditorGUILayout.EnumPopup("Trigger Mode", setting.triggerMode);
                if (setting.triggerMode is TriggerMode.DoubleFingerHold or TriggerMode.TripleFingerHold)
                {
                    setting.holdTimeForShow = EditorGUILayout.FloatField("Hold Time for show", setting.holdTimeForShow);
                    setting.holdTimeForShow = Mathf.Max(0.01f, setting.holdTimeForShow);
                }
                else if (setting.triggerMode is TriggerMode.DoubleTap or TriggerMode.TripleTap)
                {
                    setting.triggerPosition = (TriggerPosition)EditorGUILayout.EnumPopup("Trigger position", setting.triggerPosition);
                    setting.inputGraceTime = EditorGUILayout.FloatField("Input grace time", setting.inputGraceTime);
                    setting.inputGraceTime = Mathf.Max(0.01f, setting.inputGraceTime);
                }

                EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
                setting.uiFactory = (UIFactoryBase)EditorGUILayout.ObjectField("UI Factory", setting.uiFactory, typeof(UIFactoryBase), false);
                setting.tabButtonPrefab = (TabButton)EditorGUILayout.ObjectField("Tab Button Prefab", setting.tabButtonPrefab, typeof(TabButton), false);
                var serializedObject = new SerializedObject(setting);
                var topPageTabListProperty = serializedObject.FindProperty("topPageTabList");
                EditorGUILayout.PropertyField(topPageTabListProperty, true);
            }
        }
#endif
    }
}
