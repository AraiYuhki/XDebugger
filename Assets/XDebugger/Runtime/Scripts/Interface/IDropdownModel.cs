using System;
using System.Collections.Generic;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ドロップダウンコントロールのデータモデルインターフェース
    /// </summary>
    public interface IDropdownModel
    {
        /// <summary>モデルの値が変更された時に発火するイベント</summary>
        event Action Changed;
        /// <summary>現在選択されているインデックス</summary>
        int SelectedIndex { get; set; }
        /// <summary>選択肢のラベルリスト</summary>
        List<string> Labels { get; }
        /// <summary>ドロップダウンのタイトル</summary>
        string Title { get; }
        /// <summary>ビューから選択インデックスの変更を通知する</summary>
        void NotifySelectedIndexChangedFromView(int index);
        /// <summary>選択インデックスを設定し、コールバック通知の有無を指定する</summary>
        void SetSelectedIndex(int index, bool notifyCallback);
    }
}
