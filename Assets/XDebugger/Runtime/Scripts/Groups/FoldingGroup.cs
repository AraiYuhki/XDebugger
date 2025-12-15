using System;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger
{
    public class FoldingGroup : ContentGroup
    {
        [SerializeField]
        private Button headerButton;
        [SerializeField]
        private Image arrowImage;
        [SerializeField]
        private bool isFolding = false;

        private void Awake()
        {
            headerButton.onClick.AddListener(OnClickHeader);
        }

        public void Setup(IGroupModel model, ContentGroup parent, bool isFolding)
        {
            Setup(model, parent);
            SetFolding(isFolding);
        }

        public void SetFolding(bool isFolding)
        {
            this.isFolding = isFolding;
            if (model is FoldingGroupModel foldingGroupModel)
                foldingGroupModel.SetFoldingWithoutNotify(isFolding);
            ApplyFolding();
        }

        private void ApplyFolding()
        {
            arrowImage.transform.localScale = new Vector3(1f, isFolding ? -1f : 1f, 1f);
            content.gameObject.SetActive(!isFolding);
        }

        private void OnClickHeader()
        {
            SetFolding(!isFolding);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            ApplyFolding();
        }
#endif
    }
}
