using System.Collections.Generic;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// ピン止め表示1件分（ページ直下のモデル1つ）の状態。
    /// ツリー全体を生成した上で、ピン止めされていない部分を非表示にする。
    /// </summary>
    public sealed class PinContext
    {
        private readonly Dictionary<ControlModelBase, ControlBase> controls = new();

        public ControlModelBase Root { get; }

        public PinContext(ControlModelBase root) => Root = root;

        public void Register(ControlModelBase model, ControlBase control)
        {
            if (model != null && control != null)
                controls[model] = control;
        }

        /// <summary>ピン止め状態に合わせて各コントロールの表示/非表示を更新する</summary>
        public void Refresh()
        {
            foreach (var pair in controls)
            {
                if (pair.Value != null && pair.Key != Root)
                    pair.Value.gameObject.SetActive(PinManager.IsVisibleInPin(pair.Key));
            }
        }
    }
}
