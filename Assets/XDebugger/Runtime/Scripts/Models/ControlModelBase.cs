using UnityEngine;
using UnityEngine.AddressableAssets;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    public abstract class ControlModelBase
    {
        protected abstract string prefabAddress { get; }

        protected bool isInteractable = true;
        public string Title { get; protected set; } = string.Empty;
        public int Priority { get; protected set; } = 0;

        public IGroupModel Parent { get; private set; }

        public bool IsInteractable => isInteractable;

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

        public void SetInteractable(bool interactable) => isInteractable = interactable;

        protected virtual T Instantiate<T>(Transform parent) where T : ControlBase, new()
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>(prefabAddress).WaitForCompletion();
            var instance = GameObject.Instantiate(prefab, parent);
            return instance.GetComponent<T>();
        }

        public abstract ControlBase CreateControl(Transform parent, IUIFactory uiFactory);
    }
}
