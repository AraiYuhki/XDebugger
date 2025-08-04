using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xeon.XDebugger.Model
{
    public class HorizontalGroup
    {
        private PageModel page;
        private ContentGroup group;
        private IDisposable prev = null;

        public HorizontalGroup(PageModel page, IDisposable prev)
        {
            this.page = page;
            this.prev = prev;
        }

        public Transform CreateGroup(Transform parent)
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>($"XDebugger/HorizontalGroup").WaitForCompletion();
            var go = GameObject.Instantiate(prefab, parent);
            group = go.GetComponent<ContentGroup>();
            return group.transform;
        }
    }
}
