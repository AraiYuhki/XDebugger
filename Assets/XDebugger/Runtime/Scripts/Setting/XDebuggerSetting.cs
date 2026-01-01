using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Common
{
    [CreateAssetMenu(fileName = "XDebuggerSetting", menuName = "XDebugger/Setting")]
    public class XDebuggerSetting : ScriptableObject
    {
        [SerializeField]
        private UIFactoryBase uiFactory;

        [Header("MainMenu Settings")]
        [SerializeField]
        private TabButton tabButtonPrefab;
        [SerializeField]
        private StaticPageControl[] topPageTabList;

        public UIFactoryBase UIFactory => uiFactory;

        public TabButton TabButtonPrefab => tabButtonPrefab;

        public StaticPageControl[] TopPageTabList => topPageTabList;
    }
}
