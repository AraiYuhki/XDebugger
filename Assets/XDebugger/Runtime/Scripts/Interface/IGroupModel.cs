using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// コントロールをグループ化して管理するモデルのインターフェース
    /// </summary>
    public interface IGroupModel
    {
        /// <summary>親グループモデル</summary>
        IGroupModel Parent { get; }
        /// <summary>グループのタイトル</summary>
        string Title { get; }
        /// <summary>表示優先度</summary>
        int Priority { get; }
        /// <summary>子コントロールモデルの読み取り専用リスト</summary>
        IReadOnlyList<ControlModelBase> Children { get; }
        /// <summary>子コントロールモデルを追加する</summary>
        void AddChild(ControlModelBase model);
        /// <summary>グループに対応するUIコントロールを生成する</summary>
        ControlBase CreateControl(Transform parent, IUIFactory uiFactory);
    }
}