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

        [Header("Pin")]
        [SerializeField]
        private TriggerPosition pinPosition = TriggerPosition.TopRight;
        [SerializeField]
        private float pinnedWidth = 320f;
        [SerializeField, Range(0.1f, 1f)]
        private float pinnedAlpha = 0.9f;
        [SerializeField]
        private bool showPinnedToggleButton = true;
        [SerializeField]
        private KeyCode pinnedToggleKey = KeyCode.F2;

        [SerializeField]
        private TabButton tabButtonPrefab;
        [SerializeField]
        private StaticPageControl[] topPageTabList;

        public TriggerPosition TriggerPosition => triggerPosition;

        public TriggerMode TriggerMode => triggerMode;

        public float InputGraceTime => inputGraceTime;

        public float HoldTimeForShow => holdTimeForShow;

        public UIFactoryBase UIFactory => uiFactory;

        /// <summary>ピン止めUIの起点位置</summary>
        public TriggerPosition PinPosition => pinPosition;
        public float PinnedWidth => pinnedWidth;
        public float PinnedAlpha => pinnedAlpha;
        /// <summary>ピン止めUIの表示切替ボタンを画面上に出すか</summary>
        public bool ShowPinnedToggleButton => showPinnedToggleButton;
        /// <summary>ピン止めUIの表示切替キー（キーボード接続時）。None で無効</summary>
        public KeyCode PinnedToggleKey => pinnedToggleKey;

        public TabButton TabButtonPrefab => tabButtonPrefab;

        public StaticPageControl[] TopPageTabList => topPageTabList;

#if UNITY_EDITOR
        [CustomEditor(typeof(XDebuggerSetting))]
        private class XDebuggerSettingEditor : UnityEditor.Editor
        {
            private SerializedProperty topPageTabListProperty;

            private void OnEnable()
            {
                topPageTabListProperty = serializedObject.FindProperty("topPageTabList");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                var setting = target as XDebuggerSetting;

                EditorGUI.BeginChangeCheck();

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

                EditorGUILayout.LabelField("Pin Settings", EditorStyles.boldLabel);
                setting.pinPosition = (TriggerPosition)EditorGUILayout.EnumPopup("Pin position", setting.pinPosition);
                setting.pinnedWidth = Mathf.Max(100f, EditorGUILayout.FloatField("Pinned width", setting.pinnedWidth));
                setting.pinnedAlpha = EditorGUILayout.Slider("Pinned alpha", setting.pinnedAlpha, 0.1f, 1f);
                setting.showPinnedToggleButton = EditorGUILayout.Toggle("Show toggle button", setting.showPinnedToggleButton);
                setting.pinnedToggleKey = (KeyCode)EditorGUILayout.EnumPopup("Toggle key", setting.pinnedToggleKey);

                EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
                setting.uiFactory = (UIFactoryBase)EditorGUILayout.ObjectField("UI Factory", setting.uiFactory, typeof(UIFactoryBase), false);
                setting.tabButtonPrefab = (TabButton)EditorGUILayout.ObjectField("Tab Button Prefab", setting.tabButtonPrefab, typeof(TabButton), false);
                EditorGUILayout.PropertyField(topPageTabListProperty, true);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
