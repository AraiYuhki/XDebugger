using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ドロップダウン選択を提供する汎用コントロールのデータモデル。
    /// </summary>
    /// <typeparam name="T">選択肢の値の型。</typeparam>
    public class DropdownModel<T> : ControlModelBase, IDropdownModel
    {
        protected override string prefabAddress => $"XDebugger/{nameof(DropdownControl)}";

        private int selectedIndex = 0;

        private List<string> labels;
        private T[] options;

        private Action<T> onChangedValue;

        /// <summary>
        /// 現在の選択インデックス。設定時にコールバックは発火しない。
        /// </summary>
        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetSelectedIndex(value, false);
        }

        /// <summary>
        /// 現在選択中の項目の値。
        /// </summary>
        public T SelectedItem
        {
            get => options[selectedIndex];
        }

        /// <summary>
        /// ドロップダウンに表示するラベルの一覧。
        /// </summary>
        public List<string> Labels => labels;

        /// <summary>
        /// ラベルと選択肢を設定する。
        /// </summary>
        /// <param name="labels">表示ラベルの一覧。</param>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="isRefreshControl">trueの場合、コントロールを即座に更新する。</param>
        public void SetOptions(IEnumerable<string> labels, IEnumerable<T> options, bool isRefreshControl = true)
        {
            this.labels = labels.ToList();
            this.options = options.ToArray();
            if (isRefreshControl)
                NotifyChanged();
        }

        /// <summary>
        /// 選択肢を設定する。ラベルはToString()から自動生成される。
        /// </summary>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="isRefreshControl">trueの場合、コントロールを即座に更新する。</param>
        public void SetOptions(IEnumerable<T> options, bool isRefreshControl = true)
        {
            this.options = options.ToArray();
            this.labels = options.Select(option => option.ToString()).ToList();
            if (isRefreshControl)
                NotifyChanged();
        }


        /// <summary>
        /// ラベルと選択肢を指定する <see cref="DropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択インデックス。</param>
        /// <param name="labels">表示ラベルの一覧。</param>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public DropdownModel(string title, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, labels, options, onChangedValue);
        }

        /// <summary>
        /// ラベル・選択肢・親グループを指定する <see cref="DropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択インデックス。</param>
        /// <param name="labels">表示ラベルの一覧。</param>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public DropdownModel(string title, int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, priority)
        {
            Initialize(value, labels, options, onChangedValue);
        }

        /// <summary>
        /// 選択肢のみを指定する <see cref="DropdownModel{T}"/> のコンストラクタ。ラベルはToString()から自動生成される。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択インデックス。</param>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public DropdownModel(string title, int value, IEnumerable<T> options, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(value, options.Select(option => option.ToString()), options, onChangedValue);
        }

        /// <summary>
        /// 選択肢と親グループを指定する <see cref="DropdownModel{T}"/> のコンストラクタ。ラベルはToString()から自動生成される。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択インデックス。</param>
        /// <param name="options">選択肢の値の一覧。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public DropdownModel(string title, int value, IEnumerable<T> options, Action<T> onChangedValue, IGroupModel parent, int priority = 0)
            : base(title, parent, priority)
        {
            Initialize(value, options.Select(option => option.ToString()), options, onChangedValue);
        }

        private void Initialize(int value, IEnumerable<string> labels, IEnumerable<T> options, Action<T> onChangedValue)
        {
            selectedIndex = value;
            this.labels = labels.ToList();
            this.options = options.ToArray();
            this.onChangedValue = onChangedValue;
        }


        /// <inheritdoc/>
        public override ControlBase CreateControl(Transform parent, IUIFactory uiFactory)
        {
            var control = uiFactory.CreateControl<DropdownControl>(parent);
            control.Setup(this);
            return control;
        }

        /// <summary>
        /// View側から選択インデックスが変更されたことを通知し、コールバックを発火する。
        /// </summary>
        /// <param name="index">新しい選択インデックス。</param>
        public void NotifySelectedIndexChangedFromView(int index) => SetSelectedIndex(index, true);

        /// <summary>
        /// 選択インデックスを設定し、必要に応じてコールバックを発火する。
        /// </summary>
        /// <param name="index">新しい選択インデックス。</param>
        /// <param name="notifyCallback">trueの場合、値変更コールバックを発火する。</param>
        public void SetSelectedIndex(int index, bool notifyCallback)
        {
            selectedIndex = Mathf.Clamp(index, 0, GetMaxIndex());
            NotifyChanged();
            if (notifyCallback && options.Length > 0)
                onChangedValue?.Invoke(options[selectedIndex]);
        }

        private int GetMaxIndex() => Mathf.Max(0, options.Length - 1);
    }
}
