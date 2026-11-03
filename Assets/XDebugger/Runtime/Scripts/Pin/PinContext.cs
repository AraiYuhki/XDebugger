using System.Collections.Generic;
using System.Linq;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// ピン止めされた1件分の表示状態。
    /// グループをピン止めした後に、一部の子だけをピン止めから外すための除外情報を持つ。
    /// </summary>
    public sealed class PinContext
    {
        private readonly HashSet<ControlModelBase> excluded = new();
        private readonly Dictionary<ControlModelBase, ControlBase> controls = new();

        public ControlModelBase Root { get; }

        public PinContext(ControlModelBase root) => Root = root;

        public void Register(ControlModelBase model, ControlBase control)
        {
            if (model != null && control != null)
                controls[model] = control;
        }

        /// <summary>ピン止めの表示対象か（除外されておらず、グループなら表示される子が残っている）</summary>
        public bool IsVisible(ControlModelBase model)
        {
            if (excluded.Contains(model))
                return false;
            if (model is IGroupModel group && group.Children.Count > 0)
                return group.Children.Any(IsVisible);
            return true;
        }

        /// <summary>
        /// モデルをこのピン止めから外す。子が全て外れたグループは自動的に外れ、
        /// ルートが空になった場合はピン止め自体を解除する。
        /// </summary>
        public void Exclude(ControlModelBase model)
        {
            if (model == null || model == Root)
            {
                PinManager.Unpin(Root);
                return;
            }
            excluded.Add(model);
            if (!IsVisible(Root))
            {
                PinManager.Unpin(Root);
                return;
            }
            foreach (var pair in controls)
            {
                if (pair.Value != null && pair.Key != Root)
                    pair.Value.gameObject.SetActive(IsVisible(pair.Key));
            }
        }
    }
}
