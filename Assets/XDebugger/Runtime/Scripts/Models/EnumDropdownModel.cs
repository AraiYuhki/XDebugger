using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// Enum型ドロップダウンモデルの共通インターフェース。
    /// </summary>
    public interface IEnumDropdownModel
    {
        /// <summary>現在選択中のEnum値。</summary>
        Enum Value { get; }
        /// <summary>表示タイトル。</summary>
        string Title { get; }
        /// <summary>Enum値を設定する。</summary>
        void SetValue(Enum value, bool notifyCallback);
    }

    /// <summary>
    /// Enum型のドロップダウン選択を提供するコントロールのデータモデル。
    /// </summary>
    /// <typeparam name="T">Enum型。</typeparam>
    public class EnumDropdownModel<T> : ControlModelBase, IDropdownModel, IEnumDropdownModel
        where T : Enum
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
        /// 現在選択中のEnum値。
        /// </summary>
        public T Value => options[selectedIndex];

        /// <inheritdoc/>
        Enum IEnumDropdownModel.Value => Value;

        /// <summary>
        /// ドロップダウンに表示するラベルの一覧。
        /// </summary>
        public List<string> Labels => labels;

        /// <summary>
        /// インデックスで初期値を指定する <see cref="EnumDropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="selectedIndex">初期選択インデックス。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public EnumDropdownModel(string title, int selectedIndex, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(selectedIndex, onChangedValue);
        }

        /// <summary>
        /// インデックスと親グループで初期値を指定する <see cref="EnumDropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="selectedIndex">初期選択インデックス。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public EnumDropdownModel(string title, int selectedIndex, Action<T> onChangedValue, IGroupModel parent, int priority) : base(title, parent, priority)
        {
            Initialize(selectedIndex, onChangedValue);
        }

        /// <summary>
        /// Enum値で初期値を指定する <see cref="EnumDropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択のEnum値。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="priority">表示優先度。</param>
        public EnumDropdownModel(string title, T value, Action<T> onChangedValue, int priority = 0) : base(title, priority)
        {
            Initialize(IndexOf(value), onChangedValue);
        }
        /// <summary>
        /// Enum値と親グループで初期値を指定する <see cref="EnumDropdownModel{T}"/> のコンストラクタ。
        /// </summary>
        /// <param name="title">表示タイトル。</param>
        /// <param name="value">初期選択のEnum値。</param>
        /// <param name="onChangedValue">値変更時のコールバック。</param>
        /// <param name="parent">所属する親グループ。</param>
        /// <param name="priority">表示優先度。</param>
        public EnumDropdownModel(string title, T value, Action<T> onChangedValue, IGroupModel parent, int priority = 0) : base(title, parent, priority)
        {
            Initialize(IndexOf(value), onChangedValue);
        }

        private void Initialize(int selectedIndex, Action<T> onChangedValue)
        {
            labels = Enum.GetNames(typeof(T)).ToList();
            options = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            this.selectedIndex = selectedIndex;
            this.onChangedValue = onChangedValue;
        }

        private int IndexOf(T target)
        {
            foreach (var (value, index) in Enum.GetValues(typeof(T)).Cast<T>().Select((value, index) => (value, index)))
            {
                if (target.Equals(value))
                    return index;
            }
            return -1;
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

        /// <summary>
        /// 型安全なEnum値を設定する。
        /// </summary>
        /// <param name="value">設定するEnum値。</param>
        /// <param name="notifyCallback">trueの場合、値変更コールバックを発火する。</param>
        public void SetValue(T value, bool notifyCallback)
        {
            var index = IndexOf(value);
            SetSelectedIndex(index, notifyCallback);
        }

        /// <inheritdoc/>
        public void SetValue(Enum value, bool notifyCallback)
        {
            SetValue((T)value, notifyCallback);
        }

        private int GetMaxIndex() => Mathf.Max(0, options.Length - 1);
    }
}
