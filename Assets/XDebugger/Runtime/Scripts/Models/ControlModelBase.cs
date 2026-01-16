using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public abstract class ControlModelBase
    {
        protected abstract string prefabAddress { get; }

        protected bool isActive = true;
        public string Title { get; set; } = string.Empty;
        public int Priority { get; protected set; } = 0;

        public IGroupModel Parent { get; private set; }

        public event Action Changed;

        public ControlModelBase(string title, int priority = 0)
        {
            Title = title;
            Priority = priority;
        }

        public ControlModelBase(string title, IGroupModel parent, int priority = 0)
        {
            Title = title;
            Parent = parent;
            Priority = priority;
        }

        public void SetParent(IGroupModel parent) => Parent = parent;

        protected void NotifyChanged() => Changed?.Invoke();

        /// <summary>
        /// プレハブをインスタンス化します。
        /// </summary>
        /// <remarks>
        /// 注意: WaitForCompletion()は同期的にブロッキングするため、パフォーマンスに影響する可能性があります。
        /// 大量のコントロールを生成する場合や、パフォーマンスが重要な場面では、
        /// 非同期ロード（LoadAssetAsync + await）やプリロードパターンの使用を検討してください。
        /// </remarks>
        protected virtual T Instantiate<T>(Transform parent) where T : ControlBase, new()
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
            var instance = GameObject.Instantiate(prefab, parent);
            return instance.GetComponent<T>();
        }

        public abstract ControlBase CreateControl(Transform parent, IUIFactory uiFactory);
    }
}
