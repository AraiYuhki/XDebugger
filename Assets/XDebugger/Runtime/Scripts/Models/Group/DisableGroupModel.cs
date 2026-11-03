using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 無効化状態を切り替え可能なグループのデータモデル。
    /// </summary>
    public class DisableGroupModel : GroupModel
    {
        private bool isDisabled = false;
        // メニュー用・ピン用など、同一モデルから生成された全てのView
        private readonly List<DisableGroup> controls = new();

        /// <summary>
        /// グループの無効化状態。trueの場合、子コントロールが無効化される。
        /// </summary>
        public bool IsDisabled
        {
            get => isDisabled;
            set
            {
                isDisabled = value;
                controls.RemoveAll(c => c == null);
                foreach (var control in controls)
                    control.SetDisable(isDisabled);
                NotifyChanged();
            }
        }

        protected override string prefabAddress => "XDebugger/DisableGroup";

        /// <summary>
        /// <see cref="DisableGroupModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">グループの表示タイトル。</param>
        /// <param name="priority">表示優先度。</param>
        public DisableGroupModel(string title, int priority = 0) : base(title, priority)
        {
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateDisableGroup(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>());
            control.SetDisable(isDisabled);
            controls.RemoveAll(c => c == null);
            controls.Add(control);

            ResetChildren(control, uiFactory);

            return control;
        }

    }
}
