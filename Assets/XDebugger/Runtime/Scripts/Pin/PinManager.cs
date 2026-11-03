using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>現在ピン止めされている葉（ピン止めした順）</summary>
        public static IReadOnlyList<ControlModelBase> Pinned => pinned;

        /// <summary>ピン止めモード。有効な間、各コントロールにピンボタンが表示される</summary>
        public static bool PinMode { get; private set; }

        /// <summary>ピン止めUIを画面に表示するか。メニューを開かずに切り替え可能</summary>
        public static bool PinnedVisible { get; private set; } = true;

        /// <summary>葉のピン状態が変化した（leaf, isPinned）</summary>
        public static event Action<ControlModelBase, bool> PinChanged;
        public static event Action PinModeChanged;
        public static event Action PinnedVisibleChanged;

        /// <summary>ピン用コントロール生成中のコンテキスト</summary>
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

        /// <summary>
        /// ピン止めの単位となる「葉」か。グループ以外、または子を持たないグループが葉。
        /// ピン状態は葉だけが持ち、グループの状態は葉から導出する。
        /// </summary>
        public static bool IsLeaf(ControlModelBase model)
            => model is not IGroupModel group || group.Children.Count == 0;

        /// <summary>モデル自身と、その配下の全ての葉を列挙する</summary>
        public static IEnumerable<ControlModelBase> Leaves(ControlModelBase model)
        {
            if (model == null)
                yield break;
            if (IsLeaf(model))
            {
                yield return model;
                yield break;
            }
            foreach (var child in ((IGroupModel)model).Children)
                foreach (var leaf in Leaves(child))
                    yield return leaf;
        }

        /// <summary>
        /// ピン止めされているか。グループは、配下の全ての葉がピン止めされている場合のみ true。
        /// </summary>
        public static bool IsPinned(ControlModelBase model)
        {
            if (model == null)
                return false;
            foreach (var leaf in Leaves(model))
            {
                if (!pinned.Contains(leaf))
                    return false;
            }
            return true;
        }

        /// <summary>モデル自身または配下に、ピン止めされた葉が1つでもあるか</summary>
        public static bool HasPinned(ControlModelBase model)
        {
            foreach (var leaf in Leaves(model))
            {
                if (pinned.Contains(leaf))
                    return true;
            }
            return false;
        }

        /// <summary>ピン止め表示に含めるか（葉はピン止め済み、グループは表示対象の子が残っている）</summary>
        public static bool IsVisibleInPin(ControlModelBase model)
        {
            if (IsLeaf(model))
                return pinned.Contains(model);
            foreach (var child in ((IGroupModel)model).Children)
            {
                if (IsVisibleInPin(child))
                    return true;
            }
            return false;
        }

        /// <summary>親を辿った最上位のモデル（ページ直下のモデル）</summary>
        public static ControlModelBase RootOf(ControlModelBase model)
        {
            while (model?.Parent is ControlModelBase parent)
                model = parent;
            return model;
        }

        /// <summary>ピン止めする。グループは配下の全ての葉をピン止めする</summary>
        public static void Pin(ControlModelBase model)
        {
            foreach (var leaf in Leaves(model).ToList())
            {
                if (pinned.Contains(leaf))
                    continue;
                pinned.Add(leaf);
                PinChanged?.Invoke(leaf, true);
            }
        }

        /// <summary>ピン止めを解除する。グループは配下の全ての葉を解除する</summary>
        public static void Unpin(ControlModelBase model)
        {
            foreach (var leaf in Leaves(model).ToList())
            {
                if (!pinned.Remove(leaf))
                    continue;
                PinChanged?.Invoke(leaf, false);
            }
        }

        /// <summary>モデルとその配下のピン止めを全て解除する（モデル破棄時用）</summary>
        public static void UnpinTree(IEnumerable<ControlModelBase> models)
        {
            if (models == null)
                return;
            foreach (var model in models.ToList())
                Unpin(model);
        }

        /// <summary>全てピン止め済みなら解除、そうでなければ（一部のみでも）全てピン止めする</summary>
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
                // ピン止め表示内のコントロール: 表示の出し分け対象として登録する
                context.Register(model, control);
                PinButton.Create(control.transform, model, "×");
                return;
            }
            PinButton.Create(control.transform, model, "UNPIN");
        }
    }
}
