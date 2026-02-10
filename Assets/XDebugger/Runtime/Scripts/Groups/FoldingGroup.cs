using System;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger
{
    /// <summary>
    /// ヘッダークリックでコンテンツの展開・折りたたみができるグループ
    /// </summary>
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

        /// <summary>
        /// グループモデルと初期折りたたみ状態を設定する
        /// </summary>
        /// <param name="model">グループのデータモデル</param>
        /// <param name="parent">親のContentGroup</param>
        /// <param name="isFolding">trueで初期状態を折りたたみにする</param>
        public void Setup(IGroupModel model, ContentGroup parent, bool isFolding)
        {
            Setup(model, parent);
            SetFolding(isFolding);
        }

        /// <summary>
        /// 折りたたみ状態を設定し、UIに反映する
        /// </summary>
        /// <param name="isFolding">trueで折りたたむ</param>
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
