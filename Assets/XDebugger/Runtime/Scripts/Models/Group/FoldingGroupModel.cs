using System;
using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// 折りたたみ可能なグループのデータモデル。
    /// </summary>
    public class FoldingGroupModel : GroupModel
    {
        private bool isFolding = false;
        // メニュー用・ピン用など、同一モデルから生成された全てのView
        private readonly List<FoldingGroup> controls = new();

        private Action<bool> onChangedFolding; 
        
        /// <summary>
        /// 折りたたみ状態。trueの場合、グループが折りたたまれている。
        /// </summary>
        public bool IsFolding
        {
            get => isFolding;
            set
            {
                isFolding = value;
                ApplyToControls(null);
                onChangedFolding?.Invoke(value);
            }
        }

        /// <summary>
        /// 折りたたみ状態が変更されたときに発火されるイベント。
        /// </summary>
        public event Action<bool> OnChangedFolding
        {
            add
            {
                onChangedFolding -= value;
                onChangedFolding += value;
                value?.Invoke(isFolding);
            }

            remove => onChangedFolding -= value;
        }

        protected override string prefabAddress => "XDebugger/FoldGroup";

        /// <summary>
        /// <see cref="FoldingGroupModel"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">グループの表示タイトル。</param>
        /// <param name="isFolding">初期の折りたたみ状態。</param>
        /// <param name="priority">表示優先度。</param>
        public FoldingGroupModel(string title, bool isFolding, int priority = 0) : base(title, priority)
        {
            this.isFolding = isFolding;
        }

        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateFoldingGroup(parent);
            control.Setup(this, parent.GetComponent<ContentGroup>(), isFolding);
            controls.RemoveAll(c => c == null);
            controls.Add(control);

            ResetChildren(control, uiFactory);

            return control;
        }

        /// <summary>
        /// Viewの操作で折りたたみ状態が変わったことを通知する。
        /// 操作元以外のView（ピン止め側など）へ状態を同期する。
        /// </summary>
        public void NotifyFoldingFromView(FoldingGroup sender, bool isFolding)
        {
            this.isFolding = isFolding;
            ApplyToControls(sender);
            onChangedFolding?.Invoke(isFolding);
        }

        private void ApplyToControls(FoldingGroup except)
        {
            controls.RemoveAll(c => c == null);
            foreach (var control in controls)
            {
                if (control != except)
                    control.SetFolding(isFolding);
            }
        }

        /// <summary>
        /// コントロールへの通知なしに折りたたみ状態を設定する。
        /// </summary>
        /// <param name="isFolding">新しい折りたたみ状態。</param>
        public void SetFoldingWithoutNotify(bool isFolding)
        {
            this.isFolding = isFolding;
            onChangedFolding?.Invoke(isFolding);
        }
    }
}
