using System;
using System.Collections.ObjectModel;
using UnityEngine;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ページモデルの標準インターフェイス
    /// </summary>
    public interface IPageModel
    {
        /// <summary>ページのタイトル</summary>
        string Title { get; }
        /// <summary>コンテンツの配置先Transform</summary>
        Transform Content { get; set; }
        /// <summary>現在アクティブなグループモデル</summary>
        IGroupModel CurrentGroup { get; }
        /// <summary>ページに含まれるコントロールモデルの読み取り専用リスト</summary>
        ReadOnlyCollection<ControlModelBase> ModelList { get; }
        /// <summary>インデックスでコントロールモデルを取得する</summary>
        ControlModelBase this[int index] { get; }
        /// <summary>コントロールモデルの総数</summary>
        int Count { get; }

        /// <summary>UIファクトリーを使用してページを初期化する</summary>
        void Initialize(IUIFactory uiFactory);
        /// <summary>ページの更新処理を実行する</summary>
        void Update();
        /// <summary>現在のグループモデルを設定する</summary>
        void SetGroup(IGroupModel model);
        /// <summary>ページに紐づくコントロールを取得する</summary>
        MonoBehaviour GetControl();
        /// <summary>ページを開いてUIを生成する</summary>
        void OpenPage(Transform parent, IUIFactory uiFactory);
        /// <summary>ページを閉じる</summary>
        void Close(Action onClose = null);
        /// <summary>ページを表示する</summary>
        void Show(bool isRefresh = false);
        /// <summary>ページを非表示にする</summary>
        void Hide(Action onHidden = null);

        /// <summary>現在のページを再描画する</summary>
        void RefreshCurrentPage(Transform parent);
        /// <summary>ページ全体を再描画する</summary>
        void Refresh();
    }
}
