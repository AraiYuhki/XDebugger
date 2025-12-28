using UnityEngine;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Control;

namespace Xeon.XDebugger.Common
{
    [CreateAssetMenu(fileName = "XDebuggerSetting", menuName = "XDebugger/Setting")]
    public class XDebuggerSetting : ScriptableObject
    {
        [SerializeField]
        private TabButton tabButtonPrefab;
        [SerializeField]
        private StaticPageControl[] topPageTabList;

        public TabButton TabButtonPrefab => tabButtonPrefab;

        public StaticPageControl[] TopPageTabList => topPageTabList;
    }
}
