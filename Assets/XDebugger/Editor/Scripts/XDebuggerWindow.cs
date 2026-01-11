using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Profiler;

namespace Xeon.XDebugger.Editor
{
    public class XDebuggerWindow : EditorWindow
    {
        [MenuItem("Window/XDebugger")]
        public static void Open() => GetWindow<XDebuggerWindow>("XDebugger");

        private XDebugger instance;

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("XDebugger window only works in Play Mode.", MessageType.Warning);
                return;
            }

            instance ??= XDebugger.Instance;
            if (instance == null)
            {
                EditorGUILayout.HelpBox("XDebugger instance not found.", MessageType.Error);
                Repaint();
                return;
            }

            var tabList = instance.TabList.Where(tab => tab.Content is not IProfilerPage and not IConsolePage and not SystemInfoPage).ToList();
            var tabIndex = GUILayout.Toolbar(0, tabList.Select(tab => tab.Title).ToArray());
            var activeTab = tabList[tabIndex].Content;
        }
    }
}
