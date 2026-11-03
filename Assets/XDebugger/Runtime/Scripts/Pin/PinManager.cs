using System;
using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Pin
{
    /// <summary>
    /// ピン止め状態を管理する静的クラス。
    /// ピン止めの表示状態はデバッグメニューの開閉やピン止めモードとは独立している。
    /// </summary>
    public static class PinManager
    {
        private static readonly List<ControlModelBase> pinned = new();

        /// <summary>現在ピン止めされているモデル（ピン止めした順）</summary>
        public static IReadOnlyList<ControlModelBase> Pinned => pinned;

        /// <summary>ピン止めモード。有効な間、各コントロールにピンボタンが表示される</summary>
        public static bool PinMode { get; private set; }

        /// <summary>ピン止めUIを画面に表示するか。メニューを開かずに切り替え可能</summary>
        public static bool PinnedVisible { get; private set; } = true;

        /// <summary>モデルのピン状態が変化した（model, isPinned）</summary>
        public static event Action<ControlModelBase, bool> PinChanged;
        public static event Action PinModeChanged;
        public static event Action PinnedVisibleChanged;

        /// <summary>ピン用コントロール生成中のコンテキスト。生成された子には除外ボタンが付く</summary>
        internal static PinContext CurrentContext { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetState()
        {
            pinned.Clear();
            PinMode = false;
            PinnedVisible = true;
            PinChanged = null;
            PinModeChanged = null;
            PinnedVisibleChanged = null;
        }

        public static bool IsPinned(ControlModelBase model) => model != null && pinned.Contains(model);

        public static void Pin(ControlModelBase model)
        {
            if (model == null || pinned.Contains(model))
                return;
            pinned.Add(model);
            PinChanged?.Invoke(model, true);
        }

        public static void Unpin(ControlModelBase model)
        {
            if (model == null || !pinned.Remove(model))
                return;
            PinChanged?.Invoke(model, false);
        }

        /// <summary>
        /// モデルとその子孫（グループ配下）のピン止めを全て解除する。
        /// モデルが破棄される際に呼び、接続先を失ったピン止めUIを消す。
        /// </summary>
        public static void UnpinTree(IEnumerable<ControlModelBase> models)
        {
            if (models == null)
                return;
            foreach (var model in models)
            {
                if (model is IGroupModel group)
                    UnpinTree(group.Children);
                Unpin(model);
            }
        }

        public static void TogglePin(ControlModelBase model)
        {
            if (IsPinned(model))
                Unpin(model);
            else
                Pin(model);
        }

        public static void SetPinMode(bool value)
        {
            if (PinMode == value)
                return;
            PinMode = value;
            PinModeChanged?.Invoke();
        }

        public static void SetPinnedVisible(bool value)
        {
            if (PinnedVisible == value)
                return;
            PinnedVisible = value;
            PinnedVisibleChanged?.Invoke();
        }

        /// <summary>メニュー内に生成したコントロールへピンボタンを付与する</summary>
        internal static void AttachPinButton(ControlModelBase model, ControlBase control)
        {
            if (model == null || control == null)
                return;
            var context = CurrentContext;
            if (context != null)
            {
                // ピン止めされたグループ内の子: この子だけピン止めから外せる
                context.Register(model, control);
                PinExcludeButton.Create(control.transform, model, context);
                return;
            }
            PinButton.Create(control.transform, model, "UNPIN");
        }
    }
}
